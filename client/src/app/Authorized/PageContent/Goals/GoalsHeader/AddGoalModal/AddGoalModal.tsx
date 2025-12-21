import { ActionIcon } from "@mantine/core";
import { GoalType } from "~/models/goal";
import React from "react";
import SaveGoalForm from "./SaveGoalForm/SaveGoalForm";
import PayGoalForm from "./PayGoalForm/PayGoalForm";
import { PlusIcon } from "lucide-react";
import { useDisclosure } from "@mantine/hooks";
import Modal from "~/components/core/Modal/Modal";
import Select from "~/components/core/Select/Select/Select";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { useTranslation } from "react-i18next";

const AddGoalModal = (): React.ReactNode => {
  const [selectedGoalType, setSelectedGoalType] = React.useState<string | null>(
    null
  );

  const [isOpen, { open, close }] = useDisclosure();

  const { t } = useTranslation();

  const goalTypes: { label: string; value: string }[] = [
    { label: t("grow_my_funds"), value: GoalType.SaveGoal },
    { label: t("pay_off_debt"), value: GoalType.PayGoal },
  ];

  return (
    <>
      <ActionIcon size="input-sm" onClick={open}>
        <PlusIcon />
      </ActionIcon>
      <Modal
        opened={isOpen}
        onClose={close}
        title={<PrimaryText size="md">{t("add_goal")}</PrimaryText>}
      >
        <Select
          data={goalTypes}
          placeholder={t("i_want_to_set_a_goal_to")}
          value={selectedGoalType}
          onChange={(value) => setSelectedGoalType(value)}
          elevation={1}
        />
        {selectedGoalType === GoalType.SaveGoal && <SaveGoalForm />}
        {selectedGoalType === GoalType.PayGoal && <PayGoalForm />}
      </Modal>
    </>
  );
};

export default AddGoalModal;
