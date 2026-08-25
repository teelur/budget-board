import { Stack, Text } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { getCurrencySymbol } from "~/helpers/currency";
import { ITransaction } from "~/models/transaction";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import Card from "~/components/core/Card/Card";
import classes from "./LinkCandidateCard.module.css";

interface LinkCandidateCardProps {
  candidate: ITransaction;
  isSelected: boolean;
  onSelect: (id: string) => void;
}

const LinkCandidateCard = ({
  candidate,
  isSelected,
  onSelect,
}: LinkCandidateCardProps): React.ReactNode => {
  const { t } = useTranslation();
  const { dayjs, longDateFormat } = useLocale();
  const { preferredCurrency } = useUserSettings();
  const amount = `${getCurrencySymbol(preferredCurrency)}${candidate.amount.toFixed(2)}`;
  const date = dayjs(candidate.date).format(longDateFormat);

  return (
    <Card
      w="100%"
      hoverEffect
      className={classes.candidateCard}
      data-selected={isSelected ? "true" : "false"}
      onClick={(event) => {
        event.stopPropagation();
        onSelect(candidate.id);
      }}
    >
      <div className={classes.candidateContent}>
        <Stack gap={0} align="flex-start" className={classes.candidateDetails}>
          <Text size="sm" fw={600}>
            {candidate.accountName.trim() || t("unknown_account")}
          </Text>
          <Text size="xs">
            {candidate.merchantName || t("no_merchant_name")} · {date}
          </Text>
        </Stack>
        <Text size="sm" className={classes.candidateAmount}>
          {amount}
        </Text>
      </div>
    </Card>
  );
};

export default LinkCandidateCard;
