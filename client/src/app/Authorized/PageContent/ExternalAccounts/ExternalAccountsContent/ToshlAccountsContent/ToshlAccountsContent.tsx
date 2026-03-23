import { Badge, Button, Group, LoadingOverlay, Skeleton, Stack } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import { useTranslation } from "react-i18next";
import Card from "~/components/core/Card/Card";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { IApplicationUser } from "~/models/applicationUser";
import {
  IUserSettings,
  IUserSettingsUpdateRequest,
  ToshlFullSyncStatuses,
  ToshlMetadataSyncDirections,
} from "~/models/userSettings";
import { translateAxiosError } from "~/helpers/requests";
import ToshlCategoryMappingsModal from "./ToshlCategoryMappingsModal";

const ToshlAutoSyncOptions = [1, 2, 4, 8, 12, 24, 0, -1, -2];
const ToshlSyncLookbackOptions = [1, 3, 6, 12, 0];

const getMaskedToshlAccessToken = (
  suffix: string | undefined,
  totalLength: number | undefined,
): string => {
  if (!suffix || !totalLength) {
    return "";
  }

  const maskedLength = Math.max(totalLength - suffix.length, 0);
  return `${"*".repeat(maskedLength)}${suffix}`;
};

const ToshlAccountsContent = (): React.ReactNode => {
  const { t } = useTranslation();
  const { request } = useAuth();
  const [isEditingToshlAccessToken, setIsEditingToshlAccessToken] =
    React.useState(false);

  const toshlAccessTokenField = useField<string>({
    initialValue: "",
  });
  const toshlMetadataSyncDirectionField = useField<string>({
    initialValue: ToshlMetadataSyncDirections.Toshl,
  });
  const toshlAutoSyncIntervalHoursField = useField<number>({
    initialValue: 8,
  });
  const toshlSyncLookbackMonthsField = useField<number>({
    initialValue: 0,
  });

  const userQuery = useQuery({
    queryKey: ["user"],
    queryFn: async (): Promise<IApplicationUser | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/applicationUser",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IApplicationUser;
      }

      return undefined;
    },
  });

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
    refetchInterval: (query) => {
      const status = (query.state.data as IUserSettings | undefined)
        ?.toshlFullSyncStatus;
      return status === ToshlFullSyncStatuses.Queued ||
        status === ToshlFullSyncStatuses.Running
        ? 2000
        : false;
    },
  });

  React.useEffect(() => {
    if (userSettingsQuery.data?.toshlMetadataSyncDirection) {
      toshlMetadataSyncDirectionField.setValue(
        userSettingsQuery.data.toshlMetadataSyncDirection,
      );
    }
    if (
      userSettingsQuery.data?.toshlSyncLookbackMonths !== undefined &&
      userSettingsQuery.data?.toshlSyncLookbackMonths !== null
    ) {
      toshlSyncLookbackMonthsField.setValue(
        userSettingsQuery.data.toshlSyncLookbackMonths,
      );
    }
    if (
      userSettingsQuery.data?.toshlAutoSyncIntervalHours !== undefined &&
      userSettingsQuery.data?.toshlAutoSyncIntervalHours !== null
    ) {
      toshlAutoSyncIntervalHoursField.setValue(
        userSettingsQuery.data.toshlAutoSyncIntervalHours,
      );
    }
  }, [
    userSettingsQuery.data?.toshlMetadataSyncDirection,
    userSettingsQuery.data?.toshlSyncLookbackMonths,
    userSettingsQuery.data?.toshlAutoSyncIntervalHours,
    toshlMetadataSyncDirectionField.setValue,
    toshlSyncLookbackMonthsField.setValue,
    toshlAutoSyncIntervalHoursField.setValue,
  ]);

  const queryClient = useQueryClient();
  const hasLinkedToshlAccessToken = Boolean(userQuery.data?.toshlAccessToken);
  const toshlFullSyncStatus =
    userSettingsQuery.data?.toshlFullSyncStatus ?? ToshlFullSyncStatuses.Idle;
  const isToshlFullSyncActive =
    toshlFullSyncStatus === ToshlFullSyncStatuses.Queued ||
    toshlFullSyncStatus === ToshlFullSyncStatuses.Running;

  const refreshPostSyncQueries = React.useCallback((): void => {
    void Promise.all([
      queryClient.invalidateQueries({ queryKey: ["user"] }),
      queryClient.invalidateQueries({ queryKey: ["userSettings"] }),
      queryClient.invalidateQueries({ queryKey: ["accounts"] }),
      queryClient.invalidateQueries({ queryKey: ["institutions"] }),
      queryClient.invalidateQueries({ queryKey: ["transactions"] }),
      queryClient.invalidateQueries({ queryKey: ["budgets"] }),
      queryClient.invalidateQueries({ queryKey: ["assets"] }),
      queryClient.invalidateQueries({ queryKey: ["goals"] }),
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] }),
    ]);
  }, [queryClient]);

  React.useEffect(() => {
    if (userSettingsQuery.data?.toshlFullSyncStatus === ToshlFullSyncStatuses.Succeeded) {
      refreshPostSyncQueries();
    }
  }, [refreshPostSyncQueries, userSettingsQuery.data?.toshlFullSyncStatus]);

  const doSyncToshl = useMutation({
    mutationFn: async () => {
      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: {
          toshlMetadataSyncDirection:
            toshlMetadataSyncDirectionField.getValue(),
          toshlSyncLookbackMonths: toshlSyncLookbackMonthsField.getValue(),
          toshlAutoSyncIntervalHours:
            toshlAutoSyncIntervalHoursField.getValue(),
        } as IUserSettingsUpdateRequest,
      });

      return await request({
        url: "/api/toshl/sync",
        method: "POST",
      });
    },
    onSuccess: async () => {
      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("toshl_full_sync_queued", {
          defaultValue: "Toshl full sync queued. You can leave this tab and return later.",
        }),
      });
      await queryClient.invalidateQueries({ queryKey: ["userSettings"] });
      await queryClient.invalidateQueries({ queryKey: ["user"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const doSaveToshlSettings = useMutation({
    mutationFn: async () => {
      const accessToken = toshlAccessTokenField.getValue().trim();
      const direction = toshlMetadataSyncDirectionField.getValue();
      const syncLookbackMonths = toshlSyncLookbackMonthsField.getValue();
      const autoSyncIntervalHours =
        toshlAutoSyncIntervalHoursField.getValue();

      if (accessToken.length > 0) {
        await request({
          url: "/api/toshl/updateAccessToken",
          method: "PUT",
          data: { accessToken },
        });
      }

      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: {
          toshlMetadataSyncDirection: direction,
          toshlSyncLookbackMonths: syncLookbackMonths,
          toshlAutoSyncIntervalHours: autoSyncIntervalHours,
        } as IUserSettingsUpdateRequest,
      });
    },
    onSuccess: async () => {
      toshlAccessTokenField.setValue("");
      setIsEditingToshlAccessToken(false);
      notifications.show({
        color: "var(--button-color-confirm)",
        message: t("toshl_settings_saved", {
          defaultValue: "Toshl settings saved.",
        }),
      });
      await queryClient.invalidateQueries({ queryKey: ["user"] });
      await queryClient.invalidateQueries({ queryKey: ["userSettings"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  const doRemoveAccessToken = useMutation({
    mutationFn: async () =>
      await request({
        url: "/api/toshl/removeAccessToken",
        method: "POST",
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["user"] });
      await queryClient.invalidateQueries({ queryKey: ["userSettings"] });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  if (userQuery.isLoading || userSettingsQuery.isLoading) {
    return <Skeleton height={190} radius="md" />;
  }

  return (
    <Card elevation={1}>
      <LoadingOverlay visible={doSaveToshlSettings.isPending} zIndex={1000} />
      <Stack gap="0.75rem">
        <Group justify="space-between" align="flex-start">
          <Group gap="0.5rem">
            <PrimaryText size="lg">
              {t("toshl", { defaultValue: "Toshl" })}
            </PrimaryText>
            {userQuery.data?.toshlAccessToken && (
              <Badge color="var(--button-color-confirm)">{t("connected")}</Badge>
            )}
          </Group>
          {userQuery.data?.toshlAccessToken && (
            <Button
              bg="var(--button-color-destructive)"
              size="xs"
              loading={doRemoveAccessToken.isPending}
              onClick={() => doRemoveAccessToken.mutate()}
            >
              {t("remove_toshl", { defaultValue: "Remove Toshl" })}
            </Button>
          )}
        </Group>

        <DimmedText size="sm">
          {t("link_toshl_description", {
            defaultValue: "Enter your Toshl personal API key to link your account.",
          })}{" "}
          <a
            href="https://developer.toshl.com/apps/"
            target="_blank"
            rel="noopener noreferrer"
            style={{
              color: "inherit",
              textDecoration: "underline",
            }}
          >
            developer.toshl.com/apps
          </a>
          .
        </DimmedText>

        <Stack gap="0.5rem">
          <TextInput
            label={
              <PrimaryText size="sm">
                {t("toshl_api_key", { defaultValue: "Toshl API Key" })}
              </PrimaryText>
            }
            placeholder={t("toshl_api_key_placeholder", {
              defaultValue: "Enter your Toshl API key",
            })}
            value={
              isEditingToshlAccessToken || !userQuery.data?.toshlAccessToken
                ? toshlAccessTokenField.getValue()
                : getMaskedToshlAccessToken(
                    userQuery.data?.toshlAccessTokenSuffix,
                    userQuery.data?.toshlAccessTokenLength,
                  )
            }
            onFocus={() => {
              if (userQuery.data?.toshlAccessToken && !isEditingToshlAccessToken) {
                toshlAccessTokenField.setValue("");
              }
              setIsEditingToshlAccessToken(true);
            }}
            onBlur={() => {
              if (toshlAccessTokenField.getValue().trim().length === 0) {
                setIsEditingToshlAccessToken(false);
              }
            }}
            onChange={(event) => {
              setIsEditingToshlAccessToken(true);
              toshlAccessTokenField.setValue(event.currentTarget.value);
            }}
            elevation={1}
          />
          {hasLinkedToshlAccessToken && (
            <>
              <Group justify="space-between" align="center" wrap="wrap">
                <Stack gap={0}>
                  <PrimaryText size="sm">
                    {t("toshl_category_mappings", {
                      defaultValue: "Toshl Category Mappings",
                    })}
                  </PrimaryText>
                  <DimmedText size="xs">
                    {t("toshl_category_mappings_short_description", {
                      defaultValue:
                        "Override Toshl category and tag imports with explicit Budget Board category mappings.",
                    })}
                  </DimmedText>
                </Stack>
                <ToshlCategoryMappingsModal disabled={!hasLinkedToshlAccessToken} />
              </Group>
              <Group align="flex-end" wrap="nowrap">
                <Select
                  label={
                    <PrimaryText size="sm">
                      {t("toshl_metadata_sync_direction", {
                        defaultValue: "Toshl Metadata Sync Direction",
                      })}
                    </PrimaryText>
                  }
                  description={
                    <DimmedText size="xs">
                      {t("toshl_metadata_sync_direction_description", {
                        defaultValue:
                          "Choose which side should be treated as the source of truth for categories and tags.",
                      })}
                    </DimmedText>
                  }
                  data={[
                    {
                      value: ToshlMetadataSyncDirections.Toshl,
                      label: t("toshl_to_budgetboard", {
                        defaultValue: "Toshl → Budget Board",
                      }),
                    },
                    {
                      value: ToshlMetadataSyncDirections.BudgetBoard,
                      label: t("budgetboard_to_toshl", {
                        defaultValue: "Budget Board → Toshl",
                      }),
                    },
                  ]}
                  {...toshlMetadataSyncDirectionField.getInputProps()}
                  onChange={(value) => {
                    if (value) {
                      toshlMetadataSyncDirectionField.setValue(value);
                    }
                  }}
                  elevation={1}
                  style={{ flex: 1 }}
                />
                <Select
                  label={
                    <PrimaryText size="sm">
                      {t("toshl_sync_period", {
                        defaultValue: "Toshl Sync Period",
                      })}
                    </PrimaryText>
                  }
                  description={
                    <DimmedText size="xs">
                      {t("toshl_sync_period_description", {
                        defaultValue:
                          "Choose how far back the manual Toshl full sync should import transactions.",
                      })}
                    </DimmedText>
                  }
                  data={ToshlSyncLookbackOptions.map((months) => ({
                    value: String(months),
                    label:
                      months === 0
                        ? t("all_time", { defaultValue: "All Time" })
                        : months === 12
                          ? t("1_year", { defaultValue: "1 Year" })
                          : t(months === 1 ? "1_month" : `${months}_months`, {
                              defaultValue:
                                months === 1 ? "1 Month" : `${months} Months`,
                            }),
                  }))}
                  value={String(toshlSyncLookbackMonthsField.getValue())}
                  onChange={(value) => {
                    if (value) {
                      toshlSyncLookbackMonthsField.setValue(Number(value));
                    }
                  }}
                  elevation={1}
                  style={{ flex: 1 }}
                />
                <Group align="center" wrap="nowrap" style={{ flex: 1 }}>
                  <Button
                    miw={120}
                    onClick={() => doSyncToshl.mutate()}
                    loading={doSyncToshl.isPending}
                    disabled={!hasLinkedToshlAccessToken || isToshlFullSyncActive}
                  >
                    {isToshlFullSyncActive
                      ? t("toshl_full_sync_in_progress", {
                          defaultValue: "Sync in Progress",
                        })
                      : t("full_sync", { defaultValue: "Full Sync" })}
                  </Button>
                  <DimmedText
                    size="xs"
                    ta="center"
                    style={{ flex: 1, lineHeight: 1.2 }}
                  >
                    {getToshlFullSyncInlineStatusText(userSettingsQuery.data, t)}
                  </DimmedText>
                </Group>
              </Group>
              {userSettingsQuery.data?.toshlFullSyncError &&
                toshlFullSyncStatus === ToshlFullSyncStatuses.Failed && (
                  <DimmedText size="xs" c="var(--button-color-destructive)">
                    {userSettingsQuery.data.toshlFullSyncError}
                  </DimmedText>
                )}
              <Select
                label={
                  <PrimaryText size="sm">
                    {t("toshl_auto_sync_period", {
                      defaultValue: "Toshl Auto-Sync Period",
                    })}
                  </PrimaryText>
                }
                description={
                  <DimmedText size="xs">
                    {t("toshl_auto_sync_period_description", {
                      defaultValue:
                        "Budget Board will refresh Toshl metadata on this cadence in the background.",
                    })}
                  </DimmedText>
                }
                data={ToshlAutoSyncOptions.map((hours) => {
                  if (hours > 0) {
                    return {
                      value: String(hours),
                      label: t("hours_with_value", {
                        hours,
                        defaultValue: `${hours}h`,
                      }),
                    };
                  }

                  return {
                    value: String(hours),
                    label:
                      hours === 0
                        ? t("end_of_day", { defaultValue: "End of Day" })
                        : hours === -1
                          ? t("end_of_week", { defaultValue: "End of Week" })
                          : t("end_of_month", { defaultValue: "End of Month" }),
                  };
                })}
                value={String(toshlAutoSyncIntervalHoursField.getValue())}
                onChange={(value) => {
                  if (value) {
                    toshlAutoSyncIntervalHoursField.setValue(Number(value));
                  }
                }}
                elevation={1}
              />
            </>
          )}
          <Button
            onClick={() => doSaveToshlSettings.mutate()}
            loading={doSaveToshlSettings.isPending}
            disabled={isToshlFullSyncActive}
          >
            {t("save_toshl_settings", {
              defaultValue: "Save Toshl Settings",
            })}
          </Button>
        </Stack>
      </Stack>
    </Card>
  );
};

export default ToshlAccountsContent;

const getToshlFullSyncInlineStatusText = (
  userSettings: IUserSettings | undefined,
  t: ReturnType<typeof useTranslation>["t"],
): string => {
  const status = userSettings?.toshlFullSyncStatus ?? ToshlFullSyncStatuses.Idle;

  switch (status) {
    case ToshlFullSyncStatuses.Queued:
      return t("toshl_full_sync_queued_description", {
        defaultValue: "Queued",
      });
    case ToshlFullSyncStatuses.Running:
      return `${t("running", { defaultValue: "Running" })} ${
        userSettings?.toshlFullSyncProgressPercent ?? 0
      }%${
        userSettings?.toshlFullSyncProgressDescription
          ? ` · ${userSettings.toshlFullSyncProgressDescription}`
          : ""
      }`;
    case ToshlFullSyncStatuses.Succeeded:
      return userSettings?.toshlFullSyncCompletedAt
        ? `Completed at ${new Date(
            userSettings.toshlFullSyncCompletedAt,
          ).toLocaleString()}`
        : t("succeeded", { defaultValue: "Succeeded" });
    case ToshlFullSyncStatuses.Failed:
      return userSettings?.toshlFullSyncCompletedAt
        ? `Failed at ${new Date(
            userSettings.toshlFullSyncCompletedAt,
          ).toLocaleString()}`
        : t("failed", { defaultValue: "Failed" });
    default:
      return t("idle", { defaultValue: "Idle" });
  }
};
