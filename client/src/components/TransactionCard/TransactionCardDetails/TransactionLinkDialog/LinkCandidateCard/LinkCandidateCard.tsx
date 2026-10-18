import { Group, Stack } from "@mantine/core";
import { AmountText, BodyText, Card } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import { ITransaction } from "~/models/transaction";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
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
    <Card
      w="100%"
      hoverable
      selected={isSelected}
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
      <Group justify="space-between" wrap="nowrap">
        <Stack gap={0} align="flex-start">
          <BodyText tone="primary" size="sm" fw={500}>
            {candidate.accountName.trim() || t("unknown_account")}
          </BodyText>
          <BodyText tone="secondary" size="xs">
            {candidate.merchantName || t("no_merchant_name")} · {date}
          </BodyText>
        </Stack>
        <AmountText
          amount={candidate.amount}
          size="sm"
          isSensitive={isPrivacyModeEnabled}
          locale={intlLocale}
          currency={preferredCurrency}
          decimalPlaces={decimalPlaces}
        />
      </Group>
    </Card>
  );
};

export default LinkCandidateCard;
