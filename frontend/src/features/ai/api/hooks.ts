import { isDomainErrorResponse, isFetchBaseQueryError, isNotFoundErrorResponse, isServerErrorResponse, isValidationProblemDetails } from "@shared/types/ProblemDetails";
import { useEffect, useRef, useState } from "react";
import { isErrorResponse, isPendingResponse, isResultResponse, RoomOnImage } from "../models/IGetPlanLabelingResult";
import { POLLING_PERIOD_MS } from "@app/config/constants";
import aiApi from "./aiApi";

// Custom hook for image processing pipeline
export function useImageProcessingPipeline(imageBlob: Blob | null) {
  const [error, setError] = useState<string | null>(null);
  const [result, setResult] = useState<RoomOnImage[] | null>(null);
  const [requestLabeling, { data: rLData, error: rLError, isSuccess: rLIsSuccess, isError: rLIsError }] = aiApi.useRequestPlanLabelingMutation();
  const [getLabelingResult, { data: gLData, error: gLError, isSuccess: gLIsSuccess, isError: gLIsError }] = aiApi.useLazyGetPlanLabelingResultQuery();
  const pollingRef = useRef<NodeJS.Timeout | null>(null);

  // Handle image submission
  useEffect(() => {
    if (imageBlob) {
      requestLabeling({ planImage: imageBlob });
    }
  }, [imageBlob, requestLabeling]);

  // Handle request submission response
  useEffect(() => {
    if (rLIsSuccess && rLData) {
      // Start polling
      pollingRef.current = setInterval(() => {
        getLabelingResult({ requestId: rLData.requestId });
      }, POLLING_PERIOD_MS);
    }

    if (rLIsError && rLError) {
      let errorMessage = "Не удалось отправить изображение на разметку.";
      if (isValidationProblemDetails(rLError)) {
        // @ts-ignore
        errorMessage = rLError?.data?.details || "Недопустимый формат или размер изображения.";
      } else if (isDomainErrorResponse(rLError)) {
        errorMessage = rLError.data.detail || "Ошибка сервера при отправке изображения.";
      }
      setError(errorMessage);
    }

    // Cleanup polling on unmount or error
    return () => {
      if (pollingRef.current) {
        clearInterval(pollingRef.current);
        pollingRef.current = null;
      }
    };
  }, [rLData, rLError, rLIsSuccess, rLIsError, getLabelingResult]);

  // Handle polling results
  useEffect(() => {
    if (gLIsSuccess && gLData) {
      if (isPendingResponse(gLData)) {
        // Continue polling
      } else if (isResultResponse(gLData)) {
        setResult(gLData.result);
        if (pollingRef.current) {
          clearInterval(pollingRef.current);
          pollingRef.current = null;
        }
      } else if (isErrorResponse(gLData)) {
        setError(gLData.error || "Сервер вернул ошибку при обработке плана.");
        if (pollingRef.current) {
          clearInterval(pollingRef.current);
          pollingRef.current = null;
        }
      }
    }

    if (gLIsError && gLError) {
      let errorMessage = "Не удалось получить результаты разметки.";
      if (isNotFoundErrorResponse(gLError)) {
        errorMessage = gLError.data.detail || "Запрос устарел.";
      } else if (isServerErrorResponse(gLError)) {
        // @ts-ignore
        errorMessage = gLError?.data?.detail || "Ошибка сервера при получении результатов.";
      } else if (isFetchBaseQueryError(gLError)) {
        errorMessage = "Непредвиденная ошибка во время обработки изображения.";
        console.error(gLError);
      }
      setError(errorMessage);
      if (pollingRef.current) {
        clearInterval(pollingRef.current);
        pollingRef.current = null;
      }
    }
  }, [gLData, gLError, gLIsSuccess, gLIsError]);

  return { error, result };
}
