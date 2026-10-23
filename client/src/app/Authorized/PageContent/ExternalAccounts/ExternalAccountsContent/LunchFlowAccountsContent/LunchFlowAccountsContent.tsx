import { Group, Stack } from "@mantine/core";
import { Badge, Button, HeadingText, Skeleton } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import LinkLunchFlow from "./LinkLunchFlow/LinkLunchFlow";
import LunchFlowInstitutionCards from "./LunchFlowInstitutionCards/LunchFlowInstitutionCards";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";
import { useRemoveApiKeyMutation } from "~/hooks/mutations/lunchFlow/useRemoveApiKeyMutation";

const LunchFlowAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const applicationUserQuery = useApplicationUserQuery();
  const removeApiKeyMutation = useRemoveApiKeyMutation();

  const getContent = () => {
    if (applicationUserQuery.isPending) {
      return <Skeleton height={150} />;
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
          <HeadingText tone="heading" level={4}>
            {t("lunchflow")}
          </HeadingText>
          {applicationUserQuery.data?.lunchFlowApiKey && (
            <Badge variant="light" color="success" size="sm">
              {t("connected")}
            </Badge>
          )}
        </Group>
        {applicationUserQuery.data?.lunchFlowApiKey && (
          <Button
            variant="filled"
            color="error"
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
