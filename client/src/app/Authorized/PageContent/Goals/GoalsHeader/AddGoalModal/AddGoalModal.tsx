import { Group } from "@mantine/core";
import { BodyText, Button, Modal, Stepper } from "@teelur/budget-board-ui";
import React from "react";
import { PlusIcon } from "lucide-react";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import { useTranslation } from "react-i18next";
import SelectType from "./SelectType/SelectType";
import ConfigureGoal, {
  IGoalConfiguration,
} from "./ConfigureGoal/ConfigureGoal";
import SetTarget from "./SetTarget/SetTarget";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { IGoalCreateRequest } from "~/models/goal";
import { useCreateGoalMutation } from "~/hooks/mutations/goals/useCreateGoalMutation";

const AddGoalModal = (): React.ReactNode => {
  const [selectedGoalType, setSelectedGoalType] = React.useState<string | null>(
    null,
  );
  const [goalConfiguration, setGoalConfiguration] =
    React.useState<IGoalConfiguration>({
      name: "",
      accounts: [],
      targetAmount: 0,
      applyAccountAmount: false,
    });

  const [isOpen, { open, close }] = useDisclosure();
  const [activeStep, setActiveStep] = React.useState(0);

  const { dayjs } = useLocale();
  const { t } = useTranslation();
  const createGoalMutation = useCreateGoalMutation();

  useDidUpdate(() => {
    if (!isOpen) {
      setSelectedGoalType(null);
      setGoalConfiguration({
        name: "",
        accounts: [],
        targetAmount: 0,
        applyAccountAmount: false,
      });
      setActiveStep(0);
    }
  }, [isOpen]);

  const createGoal = (
    completeDate: Date | null,
    monthlyContribution: number,
  ) => {
    const parsedCompleteDate = dayjs(completeDate);
    const newGoal: IGoalCreateRequest = {
      name: goalConfiguration.name,
      completeDate: parsedCompleteDate.isValid()
        ? parsedCompleteDate.format("YYYY-MM-DD")
        : null,
      amount: goalConfiguration.targetAmount,
      applyExistingBalanceTowardsGoal: goalConfiguration.applyAccountAmount,
      monthlyContribution:
        monthlyContribution === 0 ? null : monthlyContribution,
      accountIds: goalConfiguration.accounts,
    };

    createGoalMutation.mutate(newGoal, {
      onSuccess: () => {
        setActiveStep(3);
      },
    });
  };

  return (
    <>
      <Button variant="filled" color="primary" size="compact-sm" onClick={open}>
        <PlusIcon size={22} />
      </Button>
      <Modal opened={isOpen} onClose={close} size="600px" title={t("add_goal")}>
        <Stepper active={activeStep} allowNextStepsSelect={false} w="100%">
          <Stepper.Step label={t("step_1")} description={t("select_type")}>
            <SelectType
              selectedGoalType={selectedGoalType}
              setSelectedGoalType={setSelectedGoalType}
              launchNextDialog={() => setActiveStep(1)}
            />
          </Stepper.Step>
          <Stepper.Step
            label={t("step_2")}
            description={t("configure_details")}
          >
            <ConfigureGoal
              selectedGoalType={selectedGoalType}
              goalConfiguration={goalConfiguration}
              setGoalConfiguration={setGoalConfiguration}
              goBackToPreviousDialog={() => setActiveStep(0)}
              launchNextDialog={() => setActiveStep(2)}
            />
          </Stepper.Step>
          <Stepper.Step label={t("step_3")} description={t("set_target")}>
            <SetTarget
              goBackToPreviousDialog={() => setActiveStep(1)}
              createGoal={createGoal}
              isCreatingGoal={createGoalMutation.isPending}
            />
          </Stepper.Step>
          <Stepper.Completed>
            <Group justify="center">
              <BodyText tone="secondary" size="sm">
                {t("goal_created_successfully")}
              </BodyText>
            </Group>
          </Stepper.Completed>
        </Stepper>
      </Modal>
    </>
  );
};

export default AddGoalModal;
