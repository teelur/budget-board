import { Group, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import { ITransaction } from "~/models/transaction";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import Card from "~/components/core/Card/Card";
import classes from "./LinkCandidateCard.module.css";
import { AmountText } from "@teelur/budget-board-ui";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { usePrivacyMode } from "~/providers/PrivacyModeProvider/PrivacyModeProvider";

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
  const { dayjs, longDateFormat, intlLocale } = useLocale();
  const { preferredCurrency, decimalPlaces } = useUserSettings();
  const { isPrivacyModeEnabled } = usePrivacyMode();

  const date = dayjs(candidate.date).format(longDateFormat);

  return (
    <div
      className={classes.candidateWrapper}
      role="button"
      tabIndex={0}
      aria-pressed={isSelected}
      onClick={(event) => {
        event.stopPropagation();
        onSelect(candidate.id);
      }}
      onKeyDown={(event: React.KeyboardEvent<HTMLDivElement>) => {
        if (event.key !== "Enter" && event.key !== " ") {
          return;
        }

        event.preventDefault();
        onSelect(candidate.id);
      }}
    >
      <Card
        w="100%"
        hoverEffect
        className={classes.candidateCard}
        data-selected={isSelected ? "true" : "false"}
      >
        <Group justify="space-between" wrap="nowrap">
          <Stack
            gap={0}
            align="flex-start"
            className={classes.candidateDetails}
          >
            <PrimaryText size="sm" fw={600}>
              {candidate.accountName.trim() || t("unknown_account")}
            </PrimaryText>
            <DimmedText size="xs">
              {candidate.merchantName || t("no_merchant_name")} · {date}
            </DimmedText>
          </Stack>
          <AmountText
            amount={candidate.amount}
            size="sm"
            isSensitive={isPrivacyModeEnabled}
            locale={intlLocale}
            currency={preferredCurrency}
            decimalPlaces={decimalPlaces}
            className={classes.candidateAmount}
          />
        </Group>
      </Card>
    </div>
  );
};

export default LinkCandidateCard;
