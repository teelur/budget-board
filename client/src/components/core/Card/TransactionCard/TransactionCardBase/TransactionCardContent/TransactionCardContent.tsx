import classes from "./TransactionCardContent.module.css";

import { Flex, Tooltip } from "@mantine/core";
import { Badge, AmountText } from "@teelur/budget-board-ui";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import { getFormattedCategoryValue } from "~/helpers/category";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { Repeat2 } from "lucide-react";
import { useTranslation } from "react-i18next";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

interface TransactionCardContentProps {
  transaction: ITransaction;
  categories: ICategory[];
  elevation: number;
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
    <Flex className={classes.content} w="100%" gap="0.5rem" align="center">
      <DimmedText
        className={classes.dateText}
        flex="1 0 auto"
        size="sm"
        elevation={props.elevation}
      >
        {dayjs(props.transaction.date).format(`${longDateFormat}`)}
      </DimmedText>
      <PrimaryText
        className={classes.merchantNameText}
        w="100%"
        elevation={props.elevation}
      >
        {props.transaction.merchantName}
      </PrimaryText>
      {props.transaction.recurringRuleID && (
        <Tooltip label={t("recurring")}>
          <Repeat2
            size="1.5rem"
            aria-label={t("recurring")}
            color="var(--mantine-primary-color-filled)"
          />
        </Tooltip>
      )}
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
