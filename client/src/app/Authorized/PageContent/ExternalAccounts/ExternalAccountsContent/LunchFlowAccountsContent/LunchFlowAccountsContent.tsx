import { Badge, Button, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import LinkLunchFlow from "./LinkLunchFlow/LinkLunchFlow";
import LunchFlowInstitutionCards from "./LunchFlowInstitutionCards/LunchFlowInstitutionCards";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";
import { useRemoveApiKeyMutation } from "~/hooks/mutations/lunchFlowAccount/useRemoveApiKeyMutation";

const LunchFlowAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const applicationUserQuery = useApplicationUserQuery();
  const removeApiKeyMutation = useRemoveApiKeyMutation();

  const getContent = () => {
    if (applicationUserQuery.isPending) {
      return <Skeleton height={150} radius="md" />;
    }

    if (applicationUserQuery.data?.lunchFlowApiKey) {
      return <LunchFlowInstitutionCards />;
    }

    return <LinkLunchFlow />;
  };

  return (
    <Stack p={0} gap="0.5rem">
      <Group justify="space-between">
        <Group>
          <PrimaryHeading order={4}>{t("lunchflow")}</PrimaryHeading>
          {applicationUserQuery.data?.lunchFlowApiKey && (
            <Badge color="var(--button-color-confirm)">{t("connected")}</Badge>
          )}
        </Group>
        {applicationUserQuery.data?.lunchFlowApiKey && (
          <Button
            bg="var(--button-color-destructive)"
            size="xs"
            loading={removeApiKeyMutation.isPending}
            disabled={
              removeApiKeyMutation.isPending || applicationUserQuery.isPending
            }
            onClick={() => removeApiKeyMutation.mutate()}
          >
            {t("remove_lunchflow")}
          </Button>
        )}
      </Group>
      {getContent()}
    </Stack>
  );
};

export default LunchFlowAccountsContent;
