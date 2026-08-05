import { Badge, Button, Group, Skeleton, Stack } from "@mantine/core";
import React from "react";
import { useTranslation } from "react-i18next";
import SimpleFinOrganizationCards from "./SimpleFinOrganizationCards/SimpleFinOrganizationCards";
import LinkSimpleFin from "./LinkSimpleFin/LinkSimpleFin";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
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
          <PrimaryHeading order={4}>{t("simplefin")}</PrimaryHeading>
          {applicationUserQuery.data?.simpleFinAccessToken && (
            <Badge color="var(--button-color-confirm)">{t("connected")}</Badge>
          )}
        </Group>
        {applicationUserQuery.data?.simpleFinAccessToken && (
          <Button
            bg="var(--button-color-destructive)"
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
