import { Flex, Group, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { ActionIcon, Button } from "@teelur/budget-board-ui";
import { RepeatIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import Modal from "~/components/core/Modal/Modal";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import RolloverControl from "../RolloverControl/RolloverControl";

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
        size="sm"
        title={
          <PrimaryHeading component="span" order={4}>
            {t("rollover_settings", { category: props.category })}
          </PrimaryHeading>
        }
      >
        <Stack gap="1rem">
          <Stack gap="0.5rem">
            <DimmedText size="sm">{t("rollover_description")}</DimmedText>
            <DimmedText size="sm">{t("rollover_parent_child_note")}</DimmedText>
          </Stack>
          <RolloverControl
            rolloverStartMonth={draftStartMonth}
            budgetMonth={props.budgetMonth}
            onChange={setDraftStartMonth}
            disabledReason={props.disabledReason}
            size="sm"
          />
          <Group justify="flex-end" gap="0.5rem">
            <Button variant="outline" color="primary" onClick={close}>
              {t("cancel")}
            </Button>
            <Button variant="filled" color="primary" onClick={save}>
              {t("save")}
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Flex>
  );
};

export default RolloverButton;
