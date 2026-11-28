import { Flex, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { ActionIcon, BodyText, Modal } from "@teelur/budget-board-ui";
import { RepeatIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { ModalActions } from "~/components/ModalActions/ModalActions";
import RolloverControl from "../../../RolloverControl/RolloverControl";

interface RolloverButtonProps {
  category: string;
  rolloverStartMonth: string | null;
  budgetMonth: Date;
  onChange: (rolloverStartMonth: string | null) => void;
  /** Why rollover can't be turned on here, e.g. because a related category already rolls over. */
  disabledReason?: string;
}

const RolloverButton = (props: RolloverButtonProps): React.ReactNode => {
  const [isOpen, { open, close }] = useDisclosure(false);
  const [draftStartMonth, setDraftStartMonth] = React.useState<string | null>(
    props.rolloverStartMonth,
  );

  const { t } = useTranslation();

  const openModal = () => {
    setDraftStartMonth(props.rolloverStartMonth);
    open();
  };

  const save = () => {
    if (draftStartMonth !== props.rolloverStartMonth) {
      props.onChange(draftStartMonth);
    }
    close();
  };

  return (
    // The modal renders in a portal, but its clicks still bubble through React to the budget
    // card, which would open the budget details.
    <Flex style={{ alignSelf: "stretch" }} onClick={(e) => e.stopPropagation()}>
      <ActionIcon
        variant="outline"
        color="primary"
        size="compact-sm"
        h="100%"
        selected={props.rolloverStartMonth !== null}
        aria-label={t("rollover_settings", { category: props.category })}
        title={t("rollover_settings", { category: props.category })}
        onClick={openModal}
      >
        <RepeatIcon size="1rem" />
      </ActionIcon>
      <Modal
        opened={isOpen}
        onClose={close}
        title={t("rollover_settings", { category: props.category })}
      >
        <Stack gap="1rem">
          <Stack gap="0.5rem">
            <BodyText size="sm" tone="secondary">
              {t("rollover_description")}
            </BodyText>
            <BodyText size="sm" tone="secondary">
              {t("rollover_parent_child_note")}
            </BodyText>
          </Stack>
          <RolloverControl
            rolloverStartMonth={draftStartMonth}
            budgetMonth={props.budgetMonth}
            onChange={setDraftStartMonth}
            disabledReason={props.disabledReason}
            size="sm"
          />
          <ModalActions
            actionLabel={t("save")}
            onAction={save}
            onCancel={close}
          />
        </Stack>
      </Modal>
    </Flex>
  );
};

export default RolloverButton;
