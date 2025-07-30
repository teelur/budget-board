import classes from "../TransactionCard.module.css";

import { Badge, Flex, Text } from "@mantine/core";
import { ITransaction } from "~/models/transaction";
import React from "react";
import { ICategory } from "~/models/category";
import { getFormattedCategoryValue } from "~/helpers/category";
import { convertNumberToCurrency } from "~/helpers/currency";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface TransactionCardProps {
  transaction: ITransaction;
  categories: ICategory[];
}

const UnselectedTransactionCard = (
  props: TransactionCardProps
): React.ReactNode => {
  const { request } = React.useContext<any>(AuthContext);

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

  const categoryValue =
    (props.transaction.subcategory ?? "").length > 0
      ? props.transaction.subcategory ?? ""
      : props.transaction.category ?? "";

  return (
    <Flex
      className={classes.container}
      direction={{ base: "column", transactionBreakpoint: "row" }}
    >
      <Flex
        className={classes.dateContainer}
        w={{ base: "100%", transactionBreakpoint: "135px" }}
      >
        <Text c="dimmed" size="sm" fw={600}>
          {new Date(props.transaction.date ?? 0).toLocaleDateString([], {
            year: "numeric",
            month: "long",
            day: "numeric",
          })}
        </Text>
      </Flex>
      <Flex
        className={classes.merchantContainer}
        w={{ base: "100%", transactionBreakpoint: "200px" }}
      >
        <Text fw={600}>{props.transaction.merchantName}</Text>
      </Flex>
      <Flex
        className={classes.subcontainer}
        direction={{ base: "row" }}
        style={{ flexShrink: 1 }}
        justify="space-between"
      >
        <Flex
          className={classes.categoryContainer}
          w={{ base: "100%", transactionBreakpoint: "160px" }}
          justify={{ base: "flex-start", transactionBreakpoint: "flex-end" }}
        >
          <Badge size="md">
            {getFormattedCategoryValue(categoryValue, props.categories)}
          </Badge>
        </Flex>
        <Flex
          className={classes.amountContainer}
          w={{ base: "100%", transactionBreakpoint: "140px" }}
          justify={{ base: "flex-end", transactionBreakpoint: "flex-start" }}
        >
          {userSettingsQuery.isPending ? null : (
            <Text
              style={{
                color:
                  props.transaction.amount < 0
                    ? "var(--mantine-color-red-6)"
                    : "var(--mantine-color-green-6)",
                fontWeight: 600,
              }}
            >
              {convertNumberToCurrency(
                props.transaction.amount,
                true,
                userSettingsQuery.data?.currency ?? "USD"
              )}
            </Text>
          )}
        </Flex>
      </Flex>
    </Flex>
  );
};

export default UnselectedTransactionCard;
