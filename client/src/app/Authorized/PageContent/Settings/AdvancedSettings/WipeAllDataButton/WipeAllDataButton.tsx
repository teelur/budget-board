import { Button, LoadingOverlay, Stack } from "@mantine/core";
import { notifications } from "@mantine/notifications";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import { Pages } from "~/app/Authorized/PageContent/PageContent";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { translateAxiosError } from "~/helpers/requests";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";

interface WipeAllDataButtonProps {
  setCurrentPage?: (page: Pages) => void;
}

const WipeAllDataButton = ({
  setCurrentPage,
}: WipeAllDataButtonProps): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const queryClient = useQueryClient();

  const refreshPostWipeQueries = React.useCallback((): void => {
    void Promise.all([
      queryClient.invalidateQueries({ queryKey: ["user"] }),
      queryClient.invalidateQueries({ queryKey: ["userSettings"] }),
      queryClient.invalidateQueries({ queryKey: ["accounts"] }),
      queryClient.invalidateQueries({ queryKey: ["transactions"] }),
      queryClient.invalidateQueries({ queryKey: ["budgets"] }),
      queryClient.invalidateQueries({ queryKey: ["assets"] }),
      queryClient.invalidateQueries({ queryKey: ["goals"] }),
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] }),
      queryClient.invalidateQueries({ queryKey: ["institutions"] }),
    ]);
  }, [queryClient]);

  const wipeAllData = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/applicationUser/wipeData",
        method: "POST",
      }),
    onSuccess: () => {
      setCurrentPage?.(Pages.Dashboard);
      refreshPostWipeQueries();
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  return (
    <Stack gap="0.25rem">
      <LoadingOverlay visible={wipeAllData.isPending} />
      <PrimaryText size="sm">
        {t("wipe_all_data", { defaultValue: "Wipe All Data" })}
      </PrimaryText>
      <DimmedText size="xs">
        {t("wipe_all_data_description", {
          defaultValue:
            "Delete all financial data for this account while keeping your login and settings. Use this to reset the app between import attempts.",
        })}
      </DimmedText>
      <Button
        bg="var(--button-color-destructive)"
        onClick={() => {
          const confirmed = window.confirm(
            t("wipe_all_data_confirmation", {
              defaultValue:
                "This will permanently delete your budgets, accounts, transactions, assets, goals, categories, and sync-linked data. Continue?",
            }),
          );
          if (confirmed) {
            wipeAllData.mutate();
          }
        }}
        loading={wipeAllData.isPending}
      >
        {t("wipe_all_data", { defaultValue: "Wipe All Data" })}
      </Button>
    </Stack>
  );
};

export default WipeAllDataButton;
