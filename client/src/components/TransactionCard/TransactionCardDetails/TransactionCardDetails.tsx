import classes from "./TransactionCardDetails.module.css";

import { ITransaction } from "~/models/transaction";
import React from "react";
import { Flex, Group, Stack } from "@mantine/core";
import { useTranslation } from "react-i18next";
import { Badge, BodyText } from "@teelur/budget-board-ui";

interface TransactionCardDetailsProps {
  transaction: ITransaction;
}

const TransactionCardDetails = ({
  transaction,
}: TransactionCardDetailsProps): React.ReactNode => {
  const { t } = useTranslation();
  const notes = transaction.notes.trim();

  return (
    <Flex
      pt="1rem"
      pb="0.25rem"
      px="0.5rem"
      gap={{ base: "1rem", xs: "4rem" }}
      direction={{ base: "column", xs: "row" }}
    >
      <Stack gap={0}>
        <BodyText tone="secondary" size="xs">
          {t("account")}
        </BodyText>
        <BodyText tone="primary" size="sm" fw={500} className={classes.value}>
          {transaction.accountName.trim() || t("unknown_account")}
        </BodyText>
      </Stack>
      <Stack gap={0}>
        <BodyText tone="secondary" size="xs">
          {t("notes")}
        </BodyText>
        {notes ? (
          <BodyText tone="primary" size="sm" className={classes.value}>
            {notes}
          </BodyText>
        ) : (
          <BodyText tone="muted" size="sm" className={classes.value}>
            {t("no_notes")}
          </BodyText>
        )}
      </Stack>
      <Stack gap={0}>
        <BodyText tone="secondary" size="xs">
          {t("tags")}
        </BodyText>
        {transaction.tags?.length ? (
          <Group className={classes.tags} gap="0.25rem">
            {transaction.tags.map((tag) => (
              <Badge variant="light" color="primary" size="sm" key={tag}>
                {tag}
              </Badge>
            ))}
          </Group>
        ) : (
          <BodyText tone="muted" size="sm" className={classes.value}>
            {t("no_tags")}
          </BodyText>
        )}
      </Stack>
    </Flex>
  );
};

export default TransactionCardDetails;
