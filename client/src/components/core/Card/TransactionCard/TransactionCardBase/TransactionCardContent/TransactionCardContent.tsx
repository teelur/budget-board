import classes from "./TransactionCardContent.module.css";

import { Badge, Flex, Text } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import { getFormattedCategoryValue } from "~/helpers/category";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";
import StatusText from "~/components/core/Text/StatusText/StatusText";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";

interface TransactionCardContentProps {
  transaction: ITransaction;
  categories: ICategory[];
  elevation: number;
}

const TransactionCardContent = (
  props: TransactionCardContentProps,
): React.ReactNode => {
  const { dayjs, longDateFormat, intlLocale } = useLocale();
  const { request } = useAuth();

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const getPrimaryTextColor = (): string => {
    switch (props.elevation) {
      case 0:
        return "var(--base-color-text-primary)";
      case 1:
        return "var(--surface-color-text-primary)";
      case 2:
        return "var(--elevated-color-text-primary)";
      default:
        return "var(--base-color-text-primary)";
    }
  };

  const getDimmedTextColor = (): string => {
    switch (props.elevation) {
      case 0:
        return "var(--base-color-text-dimmed)";
      case 1:
        return "var(--surface-color-text-dimmed)";
      case 2:
        return "var(--elevated-color-text-dimmed)";
      default:
        return "var(--base-color-text-dimmed)";
    }
  };

  const categoryValue =
    (props.transaction.subcategory ?? "").length > 0
      ? (props.transaction.subcategory ?? "")
      : (props.transaction.category ?? "");

  return (
    <Flex className={classes.content} w="100%" gap="0.5rem" align="center">
      <Text
        className={classes.dateText}
        flex="1 0 auto"
        c={getDimmedTextColor()}
        size="sm"
        fw={600}
      >
        {dayjs(props.transaction.date).format(`${longDateFormat}`)}
      </Text>
      <Text
        className={classes.merchantNameText}
        c={getPrimaryTextColor()}
        w="100%"
        fw={600}
      >
        {props.transaction.merchantName}
      </Text>
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
          {userSettingsQuery.isPending ? null : (
            <StatusText amount={props.transaction.amount} size="md">
              {convertNumberToCurrency(
                props.transaction.amount,
                true,
                userSettingsQuery.data?.currency ?? "USD",
                SignDisplay.Auto,
                intlLocale,
              )}
            </StatusText>
          )}
        </Flex>
      </Flex>
    </Flex>
  );
};

export default TransactionCardContent;
