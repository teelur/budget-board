import { Group, ScrollArea, SimpleGrid, Stack } from "@mantine/core";
import {
  AmountText,
  Badge,
  BodyText,
  Button,
  Card,
  Checkbox,
  MonthPickerInput,
  Table,
} from "@teelur/budget-board-ui";
import { FolderIcon, MoveLeftIcon, MoveRightIcon, TagIcon } from "lucide-react";
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

      const countCategory = (cat: IBudgetTemplateCategoryItem) => {
        count++;
        cat.subcategories?.forEach(countCategory);
      };

      template.categories.forEach((cat) => {
        countCategory(cat);

        if (cat.categoryType.toLowerCase() === "income") {
          income += cat.limit;
        } else {
          expense += cat.limit;
        }
      });

      return {
        totalIncome: income,
        totalExpense: expense,
        totalCategoriesCount: count,
      };
    }, [template]);

  const netBudget = totalIncome - totalExpense;

  return (
    <Stack gap="1rem" maw="100%" mx="auto">
      <SimpleGrid cols={{ base: 2, sm: 4 }} spacing="xs">
        <Card>
          <BodyText tone="secondary" size="xs">
            {t("categories")}
          </BodyText>
          <BodyText tone="primary" size="md" fw={600}>
            {totalCategoriesCount}
          </BodyText>
        </Card>
        <Card>
          <BodyText tone="secondary" size="xs">
            {t("income")}
          </BodyText>
          <AmountText
            amount={totalIncome}
            size="md"
            isSensitive={isPrivacyModeEnabled}
            currency={preferredCurrency}
            decimalPlaces={decimalPlaces}
            locale={intlLocale}
          />
        </Card>
        <Card>
          <BodyText tone="secondary" size="xs">
            {t("expense")}
          </BodyText>
          <AmountText
            amount={totalExpense}
            size="md"
            isSensitive={isPrivacyModeEnabled}
            currency={preferredCurrency}
            decimalPlaces={decimalPlaces}
            locale={intlLocale}
          />
        </Card>
        <Card>
          <BodyText tone="secondary" size="xs">
            {t("net")}
          </BodyText>
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
          value={dayjs(targetMonth).format("YYYY-MM-DD")}
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
        />
      </Group>
      <BodyText size="sm" fw={500} mt="0.25rem">
        {t("budget_import_preview_categories")}
      </BodyText>
      <ScrollArea h={250} type="auto">
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
                      {parent.icon ? (
                        <span>{parent.icon}</span>
                      ) : (
                        <FolderIcon size={16} aria-hidden="true" />
                      )}
                      <span>{parent.name}</span>
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Badge
                      size="sm"
                      variant="light"
                      color={
                        parent.categoryType === "income" ? "success" : "info"
                      }
                    >
                      {t(parent.categoryType)}
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
                      <Badge size="xs" color="primary" variant="outline">
                        {t("rollover")}
                      </Badge>
                    )}
                  </Table.Td>
                </Table.Tr>
                {parent.subcategories?.map((child) => (
                  <Table.Tr key={child.name}>
                    <Table.Td pl="2rem">
                      <Group gap="0.35rem" wrap="nowrap">
                        {child.icon ? (
                          <span>{child.icon}</span>
                        ) : (
                          <TagIcon size={14} aria-hidden="true" />
                        )}
                        <BodyText size="sm">{child.name}</BodyText>
                      </Group>
                    </Table.Td>
                    <Table.Td>
                      <Badge
                        size="xs"
                        variant="light"
                        color={
                          child.categoryType === "income" ? "success" : "info"
                        }
                      >
                        {t(child.categoryType)}
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
                        <Badge size="xs" color="primary" variant="outline">
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
      <Group w="100%">
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          leftSection={<MoveLeftIcon size={16} />}
          onClick={goBack}
        >
          {t("back")}
        </Button>
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          rightSection={<MoveRightIcon size={16} />}
          onClick={advanceToNext}
        >
          {t("next")}
        </Button>
      </Group>
    </Stack>
  );
};

export default PreviewBudget;
