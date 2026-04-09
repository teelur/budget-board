import {
  Group,
  LoadingOverlay,
  MantineColorScheme,
  Skeleton,
  Stack,
  useMantineColorScheme,
} from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosResponse } from "axios";
import React from "react";
import { Trans, useTranslation } from "react-i18next";
import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { translateAxiosError } from "~/helpers/requests";
import {
  DATE_SEPARATOR_PLACEHOLDER,
  DateFormatItem,
  DateFormats,
  IUserSettings,
  IUserSettingsUpdateRequest,
  LanguageItem,
  Languages,
} from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import TextInput from "~/components/core/Input/TextInput/TextInput";

const SettingsUser = (): React.ReactNode => {
  const currencyField = useField({
    initialValue: "",
  });
  const languageField = useField({
    initialValue: "",
  });
  const dateFormatField = useField({
    initialValue: "",
  });
  const dateSeparatorField = useField({
    initialValue: "",
  });

  const { t } = useTranslation();
  const { request } = useAuth();
  const { colorScheme, setColorScheme } = useMantineColorScheme();

  const darkModeOptions = [
    { label: t("auto"), value: "auto" },
    { label: t("light"), value: "light" },
    { label: t("dark"), value: "dark" },
  ];

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
  });

  const queryClient = useQueryClient();
  const doUpdateUserSettings = useMutation({
    mutationFn: async (updatedUserSettings: IUserSettingsUpdateRequest) =>
      await request({
        url: "/api/userSettings",
        method: "PUT",
        data: updatedUserSettings,
      }),
    onSuccess: async () => {
      // Need to explicitly refetch to get updated settings
      await queryClient.refetchQueries({ queryKey: ["userSettings"] });
    },
    onError: (error: any) => {
      notifications.show({
        color: "var(--button-color-destructive)",
        message: translateAxiosError(error),
      });
    },
  });

  React.useEffect(() => {
    if (userSettingsQuery.data?.currency) {
      currencyField.setValue(userSettingsQuery.data.currency);
    }
    if (userSettingsQuery.data?.language) {
      languageField.setValue(userSettingsQuery.data.language);
    }
    if (userSettingsQuery.data?.dateFormat) {
      const storedFormat = userSettingsQuery.data.dateFormat;

      // For simplicity, the dateFormatField will store the format with a placeholder for the separator
      const separatorMatch = storedFormat.match(/^[A-Z]+(.)[A-Z]+\1[A-Z]+$/);
      const separator = separatorMatch ? (separatorMatch[1] ?? "") : "";

      const placeholderFormat = separator
        ? storedFormat.replaceAll(separator, DATE_SEPARATOR_PLACEHOLDER)
        : storedFormat;

      dateFormatField.setValue(placeholderFormat);
      dateSeparatorField.setValue(separator);
    }
  }, [
    userSettingsQuery.data?.currency,
    userSettingsQuery.data?.language,
    userSettingsQuery.data?.dateFormat,
    currencyField.setValue,
    languageField.setValue,
    dateFormatField.setValue,
    dateSeparatorField.setValue,
  ]);

  return (
    <Stack gap="0.5rem">
      <LoadingOverlay visible={doUpdateUserSettings.isPending} />
      {userSettingsQuery.isPending ? (
        <Skeleton h={60} radius="md" />
      ) : (
        <Stack gap="0.25rem">
          <Select
            data={darkModeOptions}
            label={<PrimaryText size="sm">{t("appearance_mode")}</PrimaryText>}
            value={colorScheme}
            onChange={(value) => setColorScheme(value as MantineColorScheme)}
            elevation={0}
          />
          <Select
            label={
              <PrimaryText size="sm">{t("preferred_currency")}</PrimaryText>
            }
            placeholder={t("select_currency")}
            searchable
            nothingFoundMessage={t("no_currencies_found")}
            data={Intl.supportedValuesOf("currency")}
            {...currencyField.getInputProps()}
            onChange={(value) => {
              if (value) {
                doUpdateUserSettings.mutate({
                  currency: value,
                });
              }
            }}
            elevation={0}
          />
          <Select
            label={
              <Stack gap="0">
                <PrimaryText size="sm">{t("preferred_language")}</PrimaryText>
                <DimmedText size="xs">
                  <Trans
                    i18nKey="preferred_language_description"
                    components={[
                      <a
                        key="link"
                        href="https://hosted.weblate.org/engage/budget-board/"
                        target="_blank"
                        rel="noopener noreferrer"
                        style={{
                          color: "inherit",
                          textDecoration: "underline",
                        }}
                      />,
                    ]}
                  />
                </DimmedText>
              </Stack>
            }
            placeholder={t("select_your_preferred_language")}
            data={Languages.map((lang: LanguageItem) => ({
              value: lang.value,
              label: t(lang.label),
            }))}
            {...languageField.getInputProps()}
            onChange={(value) => {
              if (value) {
                doUpdateUserSettings.mutate({
                  language: value,
                });
              }
            }}
            elevation={0}
          />
          <Group gap="0.5rem" wrap="nowrap">
            <Select
              w="100%"
              label={
                <PrimaryText size="sm">
                  {t("preferred_date_format")}
                </PrimaryText>
              }
              placeholder={t("select_your_preferred_date_format")}
              data={DateFormats.map((dateFormatItem: DateFormatItem) => ({
                value: dateFormatItem.value,
                label: t(dateFormatItem.label, {
                  separator:
                    dateSeparatorField.getValue()?.length > 0
                      ? dateSeparatorField.getValue()
                      : " ",
                }),
              }))}
              {...dateFormatField.getInputProps()}
              onChange={(value) => {
                if (value === "default") {
                  dateSeparatorField.setValue("");
                }
                if (value) {
                  doUpdateUserSettings.mutate({
                    dateFormat: value.replaceAll(
                      DATE_SEPARATOR_PLACEHOLDER,
                      dateSeparatorField.getValue() || " ",
                    ),
                  });
                }
              }}
              elevation={0}
            />
            {dateFormatField.getValue() !== DateFormats.at(0)?.value && (
              <TextInput
                label={<PrimaryText size="sm">{t("separator")}</PrimaryText>}
                maw={100}
                {...dateSeparatorField.getInputProps()}
                onChange={(event) => {
                  const value = event.currentTarget.value;
                  // Only allow non-alphanumeric characters
                  if (!value || /^[^a-zA-Z0-9]$/.test(value)) {
                    dateSeparatorField.setValue(value);
                  }
                }}
                onBlur={() =>
                  doUpdateUserSettings.mutate({
                    dateFormat: dateFormatField
                      .getValue()
                      .replaceAll(
                        DATE_SEPARATOR_PLACEHOLDER,
                        dateSeparatorField.getValue() || " ",
                      ),
                  })
                }
                maxLength={1}
                elevation={0}
              />
            )}
          </Group>
        </Stack>
      )}
    </Stack>
  );
};

export default SettingsUser;
