import {
  Badge,
  Card,
  Checkbox,
  Group,
  ScrollArea,
  SimpleGrid,
  Stack,
  Table,
  Text,
} from "@mantine/core";
import { AmountText, Button, MonthPickerInput } from "@teelur/budget-board-ui";
import { ArrowLeftIcon, ArrowRightIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import {
  IBudgetTemplateCategoryItem,
  IBudgetTemplateExport,
} from "~/models/budgetTemplate";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";

interface PreviewBudgetProps {
  template: IBudgetTemplateExport;
  targetMonth: Date;
  setTargetMonth: (date: Date) => void;
  overwriteExisting: boolean;
  setOverwriteExisting: (val: boolean) => void;
  goBack: () => void;
  advanceToNext: () => void;
}

const PreviewBudget = ({
  template,
  targetMonth,
  setTargetMonth,
  overwriteExisting,
  setOverwriteExisting,
  goBack,
  advanceToNext,
}: PreviewBudgetProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, dayjsLocale, intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  // Calculate totals
  const { totalIncome, totalExpense, totalCategoriesCount } =
    React.useMemo(() => {
      let income = 0;
      let expense = 0;
      let count = 0;

      const processCategory = (cat: IBudgetTemplateCategoryItem) => {
        count++;
        // If it has subcategories, we don't double count if parent limit matches child sum
        if (cat.subcategories && cat.subcategories.length > 0) {
          cat.subcategories.forEach(processCategory);
        } else {
          if (cat.categoryType.toLowerCase() === "income") {
            income += cat.limit;
          } else {
            expense += cat.limit;
          }
        }
      };

      template.categories.forEach(processCategory);

      return {
        totalIncome: income,
        totalExpense: expense,
        totalCategoriesCount: count,
      };
    }, [template]);

  const netBudget = totalIncome - totalExpense;

  return (
    <Stack gap="1rem" w={780} maw="100%" mx="auto" pt="0.5rem">
      <SimpleGrid cols={{ base: 2, sm: 4 }} spacing="xs">
        <Card withBorder padding="xs" radius="md">
          <Text size="xs" c="dimmed">
            {t("categories")}
          </Text>
          <Text size="lg" fw={600}>
            {totalCategoriesCount}
          </Text>
        </Card>
        <Card withBorder padding="xs" radius="md">
          <Text size="xs" c="dimmed">
            {t("income")}
          </Text>
          <AmountText
            amount={totalIncome}
            size="md"
            isSensitive={isPrivacyModeEnabled}
            currency={preferredCurrency}
            decimalPlaces={decimalPlaces}
            locale={intlLocale}
          />
        </Card>
        <Card withBorder padding="xs" radius="md">
          <Text size="xs" c="dimmed">
            {t("expense")}
          </Text>
          <AmountText
            amount={totalExpense}
            size="md"
            isSensitive={isPrivacyModeEnabled}
            currency={preferredCurrency}
            decimalPlaces={decimalPlaces}
            locale={intlLocale}
          />
        </Card>
        <Card withBorder padding="xs" radius="md">
          <Text size="xs" c="dimmed">
            {t("net")}
          </Text>
          <AmountText
            amount={netBudget}
            size="md"
            isSensitive={isPrivacyModeEnabled}
            currency={preferredCurrency}
            decimalPlaces={decimalPlaces}
            locale={intlLocale}
          />
        </Card>
      </SimpleGrid>

      <Group justify="space-between" align="flex-end">
        <MonthPickerInput
          label={t("target_month")}
          description={t("budget_import_target_month_description")}
          value={targetMonth}
          onChange={(val) => {
            if (val) {
              setTargetMonth(dayjs(val).startOf("month").toDate());
            }
          }}
          locale={dayjsLocale}
          valueFormat="MMMM YYYY"
          w={240}
        />

        <Checkbox
          label={t("budget_import_overwrite_existing")}
          description={t("budget_import_overwrite_description")}
          checked={overwriteExisting}
          onChange={(e) => setOverwriteExisting(e.currentTarget.checked)}
          mb="0.25rem"
        />
      </Group>

      <Text size="sm" fw={500} mt="0.25rem">
        {t("budget_import_preview_categories")}
      </Text>

      <ScrollArea h={280} type="auto">
        <Table striped highlightOnHover withTableBorder>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>{t("category")}</Table.Th>
              <Table.Th>{t("category_type")}</Table.Th>
              <Table.Th style={{ textAlign: "right" }}>{t("limit")}</Table.Th>
              <Table.Th style={{ textAlign: "center" }}>
                {t("rollover")}
              </Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {template.categories.map((parent) => (
              <React.Fragment key={parent.name}>
                <Table.Tr fw={500}>
                  <Table.Td>
                    <Group gap="0.35rem" wrap="nowrap">
                      <span>{parent.icon || "📁"}</span>
                      <span>{parent.name}</span>
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Badge
                      size="sm"
                      variant="light"
                      color={
                        parent.categoryType === "income" ? "green" : "blue"
                      }
                    >
                      {parent.categoryType}
                    </Badge>
                  </Table.Td>
                  <Table.Td style={{ textAlign: "right" }}>
                    <AmountText
                      amount={parent.limit}
                      size="sm"
                      isSensitive={isPrivacyModeEnabled}
                      currency={preferredCurrency}
                      decimalPlaces={decimalPlaces}
                      locale={intlLocale}
                    />
                  </Table.Td>
                  <Table.Td style={{ textAlign: "center" }}>
                    {parent.rolloverEnabled && (
                      <Badge size="xs" color="teal" variant="outline">
                        {t("rollover")}
                      </Badge>
                    )}
                  </Table.Td>
                </Table.Tr>
                {parent.subcategories?.map((child) => (
                  <Table.Tr key={child.name}>
                    <Table.Td pl="2rem">
                      <Group gap="0.35rem" wrap="nowrap">
                        <span>{child.icon || "🏷️"}</span>
                        <Text size="sm">{child.name}</Text>
                      </Group>
                    </Table.Td>
                    <Table.Td>
                      <Badge
                        size="xs"
                        variant="subtle"
                        color={
                          child.categoryType === "income" ? "green" : "blue"
                        }
                      >
                        {child.categoryType}
                      </Badge>
                    </Table.Td>
                    <Table.Td style={{ textAlign: "right" }}>
                      <AmountText
                        amount={child.limit}
                        size="sm"
                        isSensitive={isPrivacyModeEnabled}
                        currency={preferredCurrency}
                        decimalPlaces={decimalPlaces}
                        locale={intlLocale}
                      />
                    </Table.Td>
                    <Table.Td style={{ textAlign: "center" }}>
                      {child.rolloverEnabled && (
                        <Badge size="xs" color="teal" variant="outline">
                          {t("rollover")}
                        </Badge>
                      )}
                    </Table.Td>
                  </Table.Tr>
                ))}
              </React.Fragment>
            ))}
          </Table.Tbody>
        </Table>
      </ScrollArea>

      <Group justify="space-between" mt="1rem">
        <Button
          variant="outline"
          color="secondary"
          leftSection={<ArrowLeftIcon size={16} />}
          onClick={goBack}
        >
          {t("back")}
        </Button>
        <Button
          variant="filled"
          color="primary"
          rightSection={<ArrowRightIcon size={16} />}
          onClick={advanceToNext}
        >
          {t("next")}
        </Button>
      </Group>
    </Stack>
  );
};

export default PreviewBudget;
