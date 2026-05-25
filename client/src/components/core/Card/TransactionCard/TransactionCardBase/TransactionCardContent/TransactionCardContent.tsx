import classes from "./TransactionCardContent.module.css";

import { Badge, Flex, Text } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import { getFormattedCategoryValue } from "~/helpers/category";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";

interface TransactionCardContentProps {
  transaction: ITransaction;
  categories: ICategory[];
  elevation: number;
  currency: string;
}

const TransactionCardContent = (
  props: TransactionCardContentProps,
): React.ReactNode => {
  const { dayjs, longDateFormat, intlLocale } = useLocale();

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
      <Flex
        className={classes.contentSubcontainer}
        gap="0.5rem"
        align="center"
        direction="row"
        justify="space-between"
      >
        <Flex className={classes.categoryContainer}>
          <Badge size="md">
            {getFormattedCategoryValue(categoryValue, props.categories)}
          </Badge>
        </Flex>
        <Flex className={classes.amountContainer}>
          <StatusText amount={props.transaction.amount} size="md">
            {convertNumberToCurrency(
              props.transaction.amount,
              true,
              props.currency,
              SignDisplay.Auto,
              intlLocale,
            )}
          </StatusText>
        </Flex>
      </Flex>
    </Flex>
  );
};

export default TransactionCardContent;
