import {
  ActionIcon,
  Badge,
  Button,
  Group,
  Skeleton,
  Stack,
  Tooltip,
} from "@mantine/core";
import { useDisclosure } from "@mantine/hooks";
import { PencilIcon, PlusIcon, TrashIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import Modal from "~/components/core/Modal/Modal";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import SensitiveAmount from "~/components/core/Text/SensitiveAmount/SensitiveAmount";
import RecurringRuleForm from "~/components/RecurringRuleForm/RecurringRuleForm";
import { useDeleteRecurringRuleMutation } from "~/hooks/mutations/recurringRules/useDeleteRecurringRuleMutation";
import { useRecurringRulesQuery } from "~/hooks/queries/useRecurringRulesQuery";
import {
  IRecurringRuleResponse,
  RecurringAmountModes,
} from "~/models/recurringRule";
import { getRecurringCadenceLabel } from "~/helpers/recurringRules";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface RecurringRuleCardProps {
  rule: IRecurringRuleResponse;
  onEdit: (rule: IRecurringRuleResponse) => void;
}

const RecurringRuleCard = (props: RecurringRuleCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, longDateFormat } = useLocale();
  const deleteMutation = useDeleteRecurringRuleMutation();
  const category =
    props.rule.subcategory ?? props.rule.category ?? t("any_category");
  const hasUnsupportedCadence = props.rule.cadence.unsupported === true;

  return (
    <Card elevation={1}>
      <Group justify="space-between" align="flex-start" wrap="nowrap">
        <Stack gap="0.25rem" miw={0}>
          <Group gap="0.5rem">
            <PrimaryText>
              {props.rule.merchantName || t("any_merchant")}
            </PrimaryText>
            <Badge variant="light">
              {getRecurringCadenceLabel(props.rule.cadence, t)}
            </Badge>
            {!props.rule.isActive && (
              <Badge color="gray">{t("inactive")}</Badge>
            )}
          </Group>
          <DimmedText size="sm">
            {props.rule.accountName} · {category}
          </DimmedText>
          <Group gap="0.75rem">
            <DimmedText size="sm">
              {props.rule.amountMode === RecurringAmountModes.Automatic
                ? t("automatic_amount")
                : t("fixed_amount")}
              :
            </DimmedText>
            <PrimaryText size="sm">
              <SensitiveAmount amount={props.rule.amount} />
            </PrimaryText>
            <DimmedText size="sm">
              {t("recurring_matched_transactions", {
                count: props.rule.matchedTransactionCount,
              })}
            </DimmedText>
          </Group>
          {props.rule.nextOccurrenceDate && (
            <DimmedText size="sm">
              {t("recurring_next_occurrence", {
                date: dayjs(props.rule.nextOccurrenceDate).format(
                  longDateFormat,
                ),
              })}
            </DimmedText>
          )}
        </Stack>
        <Group gap="0.25rem" wrap="nowrap">
          <Tooltip
            label={
              hasUnsupportedCadence
                ? t("recurring_cadence_unsupported_edit")
                : t("edit")
            }
          >
            <ActionIcon
              aria-label={t("edit")}
              disabled={hasUnsupportedCadence}
              onClick={() => props.onEdit(props.rule)}
            >
              <PencilIcon size="1rem" />
            </ActionIcon>
          </Tooltip>
          <Tooltip label={t("delete")}>
            <ActionIcon
              aria-label={t("delete")}
              color="var(--button-color-destructive)"
              loading={deleteMutation.isPending}
              onClick={() => deleteMutation.mutate(props.rule.id)}
            >
              <TrashIcon size="1rem" />
            </ActionIcon>
          </Tooltip>
        </Group>
      </Group>
    </Card>
  );
};

const RecurringRules = (): React.ReactNode => {
  const { t } = useTranslation();
  const [opened, { open, close }] = useDisclosure(false);
  const [editingRule, setEditingRule] =
    React.useState<IRecurringRuleResponse>();
  const rulesQuery = useRecurringRulesQuery();

  const openCreate = () => {
    setEditingRule(undefined);
    open();
  };

  const openEdit = (rule: IRecurringRuleResponse) => {
    setEditingRule(rule);
    open();
  };

  return (
    <Stack gap="0.5rem">
      <DimmedText size="sm">{t("recurring_rules_description")}</DimmedText>
      <Button leftSection={<PlusIcon size="1rem" />} onClick={openCreate}>
        {t("add_recurring_rule")}
      </Button>
      {rulesQuery.isPending ? (
        <Skeleton height={50} />
      ) : rulesQuery.data?.length ? (
        rulesQuery.data.map((rule) => (
          <RecurringRuleCard key={rule.id} rule={rule} onEdit={openEdit} />
        ))
      ) : (
        <Group justify="center" p="1rem">
          <DimmedText size="sm">{t("no_recurring_rules")}</DimmedText>
        </Group>
      )}
      <Modal
        opened={opened}
        onClose={close}
        title={
          <PrimaryHeading order={4}>
            {editingRule ? t("edit_recurring_rule") : t("add_recurring_rule")}
          </PrimaryHeading>
        }
      >
        <RecurringRuleForm
          key={editingRule?.id ?? "new"}
          rule={editingRule}
          onSuccess={close}
          onCancel={close}
        />
      </Modal>
    </Stack>
  );
};

export default RecurringRules;
