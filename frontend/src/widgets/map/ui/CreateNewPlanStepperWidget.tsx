import Box from '@mui/material/Box';
import Stepper from '@mui/material/Stepper';
import Step from '@mui/material/Step';
import StepLabel from '@mui/material/StepLabel';
import Button from '@mui/material/Button';
import Typography from '@mui/material/Typography';
import { useAppDispatch, useAppSelector } from '@shared/hooks/reduxTypedHooks';
import { CreateNewPlanStep, focusOnFeature, setStep } from '../lib/createNewPlanSlice';
import { useEffect } from 'react';

const steps = ['Здание', 'Помещения', 'Инфраструктура'];

function getDescription(step: CreateNewPlanStep): string {
  switch(step) {
    case CreateNewPlanStep.BuildingBoundariesSetup:
      return "Отредактируйте границы здания."
    case CreateNewPlanStep.RoomsBoundariesSetup:
      return "Отредактируйте комнаты на этажах."
    case CreateNewPlanStep.InfrastructureSetup:
      return "Добавьте информацию об оборудовании."
    default:
      return ""
  }
}

interface CreateNewPlanStepperWidgetProps {
  onComplete(): void;
  completeButtonDisabled: boolean;
  backwardButtonDisabled: boolean;
}

export default function CreateNewPlanStepperWidget({onComplete, completeButtonDisabled, backwardButtonDisabled}: CreateNewPlanStepperWidgetProps) {
  const currentStep = useAppSelector(state => state.createNewPlanReducer.currentStep);
  const dispatch = useAppDispatch();

  const handleNext = function() {
    const nextStep = currentStep + 1;
    if (nextStep > CreateNewPlanStep.InfrastructureSetup) {
      return;
    }
    dispatch(setStep(nextStep));
  }

  const handleBack = function() {
    const prevStep = currentStep - 1;
    if (prevStep < CreateNewPlanStep.BuildingBoundariesSetup) {
      return;
    }
    dispatch(setStep(prevStep));
  }

  useEffect(() => {
    dispatch(focusOnFeature());
  }, [currentStep, dispatch]);

  return (
    <Box sx={{ width: '100%' }}>
      <Stepper activeStep={currentStep}>
        {steps.map((label, index) => {
          return (
            <Step key={label} completed={index < currentStep}>
              <StepLabel>{label}</StepLabel>
            </Step>
          );
        })}
      </Stepper>
      <>
        <Typography sx={{ mt: 2, mb: 1 }}>Шаг {currentStep + 1}: {getDescription(currentStep)}</Typography>
        <Box sx={{ display: 'flex', flexDirection: 'row', pt: 2 }}>
          <Button
            color="inherit"
            disabled={currentStep === CreateNewPlanStep.BuildingBoundariesSetup || backwardButtonDisabled}
            onClick={handleBack}
            sx={{ mr: 1 }}
          >
            Назад
          </Button>
          <Box sx={{ flex: '1 1 auto' }} />
          <Button 
            onClick={currentStep === CreateNewPlanStep.InfrastructureSetup ? onComplete : handleNext}
            disabled={currentStep === CreateNewPlanStep.InfrastructureSetup && completeButtonDisabled}
          >
            {currentStep === CreateNewPlanStep.InfrastructureSetup ? 'Завершить' : 'Далее'}
          </Button>
        </Box>
        </>
    </Box>
  );
}
