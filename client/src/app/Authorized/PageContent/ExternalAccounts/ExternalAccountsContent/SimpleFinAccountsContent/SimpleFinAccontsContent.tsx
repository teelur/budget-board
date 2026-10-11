import { Group, Skeleton, Stack } from "@mantine/core";
import { Badge, Button, HeadingText } from "@teelur/budget-board-ui";
import React from "react";
import { useTranslation } from "react-i18next";
import SimpleFinOrganizationCards from "./SimpleFinOrganizationCards/SimpleFinOrganizationCards";
import LinkSimpleFin from "./LinkSimpleFin/LinkSimpleFin";
import { useApplicationUserQuery } from "~/hooks/queries/useApplicationUserQuery";
import { useRemoveAccessTokenMutation } from "~/hooks/mutations/simpleFin/useRemoveAccessTokenMutation";

const SimpleFinAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const applicationUserQuery = useApplicationUserQuery();
  const removeAccessTokenMutation = useRemoveAccessTokenMutation();

  const getContent = () => {
    if (applicationUserQuery.isPending) {
      return <Skeleton height={150} radius="md" />;
    }

    if (applicationUserQuery.data?.simpleFinAccessToken) {
      return <SimpleFinOrganizationCards />;
    }

    return <LinkSimpleFin />;
  };

  return (
    <Stack p={0} gap="0.5rem">
      <Group justify="space-between">
        <Group>
          <HeadingText tone="heading" level={4}>
            {t("simplefin")}
          </HeadingText>
          {applicationUserQuery.data?.simpleFinAccessToken && (
            <Badge variant="light" color="success" size="xs">
              {t("connected")}
            </Badge>
          )}
        </Group>
        {applicationUserQuery.data?.simpleFinAccessToken && (
          <Button
            variant="filled"
            color="error"
            size="xs"
            loading={removeAccessTokenMutation.isPending}
            disabled={
              removeAccessTokenMutation.isPending ||
              applicationUserQuery.isPending
            }
            onClick={() => removeAccessTokenMutation.mutate()}
          >
            {t("remove_simplefin")}
          </Button>
        )}
      </Group>
      {getContent()}
    </Stack>
  );
};

export default SimpleFinAccountsContent;
