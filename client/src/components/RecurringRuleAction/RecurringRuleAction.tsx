import { Button, Group, SegmentedControl, Stack } from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { Repeat2Icon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import ModalContentHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import RecurringRuleForm from "~/components/RecurringRuleForm/RecurringRuleForm";
import { useAssignRecurringTransactionMutation } from "~/hooks/mutations/recurringRules/useAssignRecurringTransactionMutation";
import { useUnassignRecurringTransactionMutation } from "~/hooks/mutations/recurringRules/useUnassignRecurringTransactionMutation";
import { useRecurringRulesQuery } from "~/hooks/queries/useRecurringRulesQuery";
import { ITransaction } from "~/models/transaction";
import Modal from "../core/Modal/Modal";
import Select from "../core/Select/Select/Select";

interface RecurringRuleActionProps {
  transaction: ITransaction;
}

type RecurringRuleMode = "new" | "existing";

const RecurringRuleAction = (
  props: RecurringRuleActionProps,
): React.ReactNode => {
  const { t } = useTranslation();
  const [opened, { open, close }] = useDisclosure(false);
  const [selectedRuleID, setSelectedRuleID] = React.useState<string | null>(
    null,
  );
  const [mode, setMode] = React.useState<RecurringRuleMode>("new");
  const rulesQuery = useRecurringRulesQuery();
  const assignMutation = useAssignRecurringTransactionMutation();
  const unassignMutation = useUnassignRecurringTransactionMutation();

  if (props.transaction.recurringRuleID) {
    return (
      <Button
        size="compact-sm"
        variant="subtle"
        loading={unassignMutation.isPending}
        onClick={() => unassignMutation.mutate(props.transaction.id)}
      >
        {t("remove_recurring_rule")}
      </Button>
    );
  }

  const assignExistingRule = () => {
    if (!selectedRuleID) {
      return;
    }

    assignMutation.mutate(
      {
        recurringRuleID: selectedRuleID,
        transactionID: props.transaction.id,
      },
      { onSuccess: closeModal },
    );
  };

  const closeModal = () => {
    setMode("new");
    setSelectedRuleID(null);
    close();
  };

  const hasExistingRules = (rulesQuery.data ?? []).length > 0;

  return (
    <>
      <Button
        size="compact-sm"
        variant="subtle"
        leftSection={<Repeat2Icon size="0.85rem" />}
        onClick={() => {
          setMode("new");
          setSelectedRuleID(null);
          open();
        }}
      >
        {t("mark_as_recurring")}
      </Button>
      <Modal
        opened={opened}
        onClose={closeModal}
        title={
          <ModalContentHeading order={4}>
            {mode === "new"
              ? t("mark_as_recurring")
              : t("use_existing_recurring_rule")}
          </ModalContentHeading>
        }
      >
        <Stack gap="0.75rem">
          {hasExistingRules && (
            <SegmentedControl
              fullWidth
              value={mode}
              onChange={(value) => setMode(value as RecurringRuleMode)}
              data={[
                { value: "new", label: t("add_recurring_rule") },
                {
                  value: "existing",
                  label: t("use_existing_recurring_rule"),
                },
              ]}
            />
          )}
          {mode === "new" ? (
            <RecurringRuleForm
              transaction={props.transaction}
              onSuccess={closeModal}
              onCancel={closeModal}
            />
          ) : (
            <>
              <Select
                label={t("use_existing_recurring_rule")}
                placeholder={t("select_existing_recurring_rule")}
                data={rulesQuery.data?.map((rule) => ({
                  value: rule.id,
                  label: `${rule.merchantName || t("any_merchant")} · ${rule.accountName}`,
                }))}
                value={selectedRuleID}
                onChange={setSelectedRuleID}
                searchable
                clearable
                elevation={0}
              />
              <Group>
                <Button
                  variant="outline"
                  disabled={!selectedRuleID}
                  loading={assignMutation.isPending}
                  onClick={assignExistingRule}
                >
                  {t("assign_recurring_rule")}
                </Button>
              </Group>
            </>
          )}
        </Stack>
      </Modal>
    </>
  );
};

export default RecurringRuleAction;
