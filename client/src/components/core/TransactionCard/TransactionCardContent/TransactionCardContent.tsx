import classes from "./TransactionCardContent.module.css";

import { Flex, Group, Tooltip } from "@mantine/core";
import { Badge, AmountText, BodyText } from "@teelur/budget-board-ui";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import { getFormattedCategoryValue } from "~/helpers/category";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { Repeat2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface TransactionCardContentProps {
  transaction: ITransaction;
  categories: ICategory[];
}

const TransactionCardContent = (
  props: TransactionCardContentProps,
): React.ReactNode => {
  const { dayjs, longDateFormat, intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { t } = useTranslation();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  const categoryValue =
    (props.transaction.subcategory ?? "").length > 0
      ? (props.transaction.subcategory ?? "")
      : (props.transaction.category ?? "");

  return (
    <Flex w="100%" gap="0.5rem" align="center" className={classes.content}>
      <BodyText tone="secondary" flex="1 0 auto" className={classes.dateText}>
        {dayjs(props.transaction.date).format(`${longDateFormat}`)}
      </BodyText>
      <Group w="100%" justify="space-between" align="center" wrap="nowrap">
        <BodyText className={classes.merchantNameText}>
          {props.transaction.merchantName}
        </BodyText>
        {props.transaction.recurringRuleID && (
          <Tooltip label={t("recurring")}>
            <Repeat2
              size="1.2rem"
              aria-label={t("recurring")}
              color="var(--bbui-color-primary)"
            />
          </Tooltip>
        )}
      </Group>
      <Flex
        className={classes.contentSubcontainer}
        gap="0.5rem"
        align="center"
        direction="row"
        justify="space-between"
      >
        <Flex className={classes.categoryContainer}>
          <Badge variant="filled" color="primary" size="xs">
            {getFormattedCategoryValue(categoryValue, props.categories)}
          </Badge>
        </Flex>
        <Flex className={classes.amountContainer}>
          <AmountText
            amount={props.transaction.amount}
            size="md"
            isSensitive={isPrivacyModeEnabled}
            locale={intlLocale}
            currency={preferredCurrency}
            decimalPlaces={decimalPlaces}
          />
        </Flex>
      </Flex>
    </Flex>
  );
};

export default TransactionCardContent;
