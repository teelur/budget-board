import { ActionIcon, Stepper } from "@mantine/core";
import React from "react";
import { PlusIcon } from "lucide-react";
import { useDisclosure } from "@mantine/hooks";
import Modal from "~/components/core/Modal/Modal";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";
import SelectType from "./SelectType/SelectType";
import ConfigureGoal from "./ConfigureGoal/ConfigureGoal";

const AddGoalModal = (): React.ReactNode => {
  const [selectedGoalType, setSelectedGoalType] = React.useState<string | null>(
    null,
  );

  const [isOpen, { open, close }] = useDisclosure();
  const [activeStep, setActiveStep] = React.useState(0);

  const { t } = useTranslation();

  return (
    <>
      <ActionIcon size="input-sm" onClick={open}>
        <PlusIcon />
      </ActionIcon>
      <Modal
        opened={isOpen}
        onClose={close}
        size="600px"
        title={<PrimaryText size="md">{t("add_goal")}</PrimaryText>}
      >
        <Stepper
          active={activeStep}
          allowNextStepsSelect={false}
          w="100%"
          mb="1rem"
        >
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
              goBackToPreviousDialog={() => setActiveStep(0)}
              launchNextDialog={() => setActiveStep(2)}
            />
          </Stepper.Step>
          <Stepper.Step label={t("step_3")} description={t("set_target")}>
            <p>Fartus</p>
          </Stepper.Step>
        </Stepper>
      </Modal>
    </>
  );
};

export default AddGoalModal;
