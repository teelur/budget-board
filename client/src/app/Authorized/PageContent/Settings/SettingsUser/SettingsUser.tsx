import {
  Group,
  LoadingOverlay,
  MantineColorScheme,
  Stack,
  useMantineColorScheme,
} from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";
import { Trans, useTranslation } from "react-i18next";
import {
  DATE_SEPARATOR_PLACEHOLDER,
  DateFormatItem,
  DateFormats,
  LanguageItem,
  Languages,
} from "~/models/userSettings";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import Select from "~/components/core/Select/Select/Select";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import { useUserSettings } from "~/providers/UserSettingsProvider/UserSettingsProvider";
import { useUpdateUserSettingsMutation } from "~/hooks/mutations/userSettings/useUpdateUserSettingsMutation";

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
  const { preferredCurrency, preferredLanguage, preferredDateFormat } =
    useUserSettings();
  const { colorScheme, setColorScheme } = useMantineColorScheme();
  const updateUserSettingsMutation = useUpdateUserSettingsMutation();

  const darkModeOptions = [
    { label: t("auto"), value: "auto" },
    { label: t("light"), value: "light" },
    { label: t("dark"), value: "dark" },
  ];

  React.useEffect(() => {
    currencyField.setValue(preferredCurrency);
  }, [preferredCurrency]);

  React.useEffect(() => {
    languageField.setValue(preferredLanguage);
  }, [preferredLanguage]);

  React.useEffect(() => {
    const storedFormat = preferredDateFormat;

    // For simplicity, the dateFormatField will store the format with a placeholder for the separator
    const separatorMatch = storedFormat.match(/^[A-Z]+(.)[A-Z]+\1[A-Z]+$/);
    const separator = separatorMatch ? (separatorMatch[1] ?? "") : "";

    const placeholderFormat = separator
      ? storedFormat.replaceAll(separator, DATE_SEPARATOR_PLACEHOLDER)
      : storedFormat;

    dateFormatField.setValue(placeholderFormat);
    dateSeparatorField.setValue(separator);
  }, [preferredDateFormat]);

  return (
    <Stack gap="0.5rem">
      <LoadingOverlay visible={updateUserSettingsMutation.isPending} />
      <Stack gap="0.25rem">
        <Select
          data={darkModeOptions}
          label={<PrimaryText size="sm">{t("appearance_mode")}</PrimaryText>}
          value={colorScheme}
          onChange={(value) => setColorScheme(value as MantineColorScheme)}
          elevation={0}
        />
        <Select
          label={<PrimaryText size="sm">{t("preferred_currency")}</PrimaryText>}
          placeholder={t("select_currency")}
          searchable
          nothingFoundMessage={t("no_currencies_found")}
          data={Intl.supportedValuesOf("currency")}
          {...currencyField.getInputProps()}
          onChange={(value) => {
            if (value) {
              updateUserSettingsMutation.mutate({
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
              updateUserSettingsMutation.mutate({
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
              <PrimaryText size="sm">{t("preferred_date_format")}</PrimaryText>
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
                updateUserSettingsMutation.mutate({
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
                updateUserSettingsMutation.mutate({
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
    </Stack>
  );
};

export default SettingsUser;
