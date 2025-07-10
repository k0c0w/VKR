import { authApi, authenticateUser, isSignInArgsValidationProblem } from "@features/authorization";
import LoginForm from "./LoginForm";
import { useEffect, useState } from "react";
import { useAppDispatch } from "@shared/hooks/reduxTypedHooks";
import { isUnauthorizedResponse } from "@shared/types/ProblemDetails";

interface LoginWidgetProps {
    afterLoginSubmit?: () => void;
}
export default function LoginWidget({afterLoginSubmit}:LoginWidgetProps) {
  const [signIn, { data, error, isSuccess, isLoading, isError }] = authApi.useSignInMutation();
  const [errors, setErrors] = useState<{globalErrorMessage?: string; login?: string[]; password?: string[];}>({
    globalErrorMessage: undefined,
    login: undefined,
    password: undefined,
  });
  const [login, setLogin] = useState<string | undefined>();
  const dispatch = useAppDispatch();

  function onSubmit(formArgs: { login: string; password: string }) {
    signIn(formArgs);
    setLogin(formArgs.login);
    if (afterLoginSubmit) {
        afterLoginSubmit();
    }

    setErrors({});
  }

  useEffect(() => {
    if (isSuccess && data && login) {
      dispatch(
        authenticateUser({
          currentUser:{
            email: login,
            roles: data.roles,
          },
          sessionInfo: {
            p1: data.p1,
            p2: data.p2,
            p_h: data.p_h
          }
        })
      );
      setErrors({ globalErrorMessage: undefined, login: undefined, password: undefined });
      return;
    }

    if (isError && error) {
      if (isUnauthorizedResponse(error)) {
        setErrors({
            globalErrorMessage: "Ошибка авторизации. Проверьте логин и пароль."
        });
        return;
      }
      if (isSignInArgsValidationProblem(error)) {
        setErrors({
          login: error.data.errors.email,
          password: error.data.errors.password,
        });
        return;
      }
      setErrors({
        globalErrorMessage: 'Непредвиденная ошибка на сервере. Пожалуйста, попробуйте позже.',
        login: undefined,
        password: undefined,
      });
    }
  }, [data, isSuccess, isError, error, login, dispatch]);

  return <LoginForm disabled={isLoading} onSubmit={onSubmit} initialErrors={errors} />;
}