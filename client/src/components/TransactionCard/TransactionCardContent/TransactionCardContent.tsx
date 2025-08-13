import classes from "./TransactionCardContent.module.css";

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

interface TransactionCardContentProps {
  transaction: ITransaction;
  categories: ICategory[];
}

const TransactionCardContent = (
  props: TransactionCardContentProps
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
    <Flex className={classes.content} w="100%" gap="0.5rem" align="center">
      <Text
        className={classes.dateText}
        flex="1 0 auto"
        c="dimmed"
        size="sm"
        fw={600}
      >
        {new Date(props.transaction.date ?? 0).toLocaleDateString([], {
          year: "numeric",
          month: "long",
          day: "numeric",
        })}
      </Text>
      <Text className={classes.merchantNameText} w="100%" fw={600}>
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
            <Text c={props.transaction.amount < 0 ? "red" : "green"} fw={600}>
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

export default TransactionCardContent;
