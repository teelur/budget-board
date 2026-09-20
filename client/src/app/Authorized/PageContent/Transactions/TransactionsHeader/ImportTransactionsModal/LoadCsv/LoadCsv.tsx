import { Group, LoadingOverlay, Stack, Switch } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { useField } from "@mantine/form";
import { NotificationType, showNotification } from "~/helpers/notifications";
import Papa from "papaparse";
import React from "react";
import { useTranslation } from "react-i18next";
import FileInput from "~/components/core/Input/FileInput/FileInput";
import TextInput from "~/components/core/Input/TextInput/TextInput";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

export type CsvRow = Record<string, unknown> & { uid: number };

interface LoadCsvProps {
  loadCsv: (headers: string[], rows: CsvRow[]) => void;
  launchNextDialog: () => void;
}

const LoadCsv = (props: LoadCsvProps): React.ReactNode => {
  const [isPending, startTransition] = React.useTransition();

  const { t } = useTranslation();

  const fileField = useField<File | null>({
    initialValue: null,
    validateOnBlur: true,
    validate: (value) => {
      if (!value) {
        return;
      }

      const nameIsCsv = value.name?.toLowerCase().endsWith(".csv");
      if (!nameIsCsv) {
        return t("file_must_be_csv_message", {
          fileType: value.type || "unknown",
          fileName: value.name,
        });
      }

      return null;
    },
  });
  const delimiterField = useField<string>({
    initialValue: ",",
    validateOnBlur: true,
    validate: (value) => {
      if (useDelimiter.getValue() === false) {
        return null;
      }
      if (!value) {
        return t("delimiter_required_message");
      }
      if (value.length > 1) {
        return t("delimiter_must_be_single_character_message");
      }
      return null;
    },
  });
  const useDelimiter = useField<boolean>({
    initialValue: false,
  });

  const loadCsv = async (
    file: File,
    delimiter: string | null,
  ): Promise<boolean> => {
    try {
      const delimitersToGuess = [
        ",",
        "\t",
        "|",
        ";",
        Papa.RECORD_SEP,
        Papa.UNIT_SEP,
      ];
      if (delimiter && delimiter.length > 0) {
        delimitersToGuess.push(delimiter);
      }

      const text = await file.text();
      const parsedText = Papa.parse(text, {
        header: true,
        skipEmptyLines: true,
        dynamicTyping: true,
        delimitersToGuess,
      });

      // Display any errors that occurred during parsing.
      if (parsedText.errors.length > 0) {
        const uniqueErrorMessages = Array.from(
          new Set(parsedText.errors.map((error) => error.message)),
        );

        uniqueErrorMessages.forEach((errorMessage) => {
          showNotification({
            type: NotificationType.Error,
            message: t("error_parsing_csv_message", { errorMessage }),
          });
        });
        return false;
      }

      if (parsedText.data.length === 0) {
        showNotification({
          type: NotificationType.Error,
          message: t("csv_file_is_empty_message"),
        });
        return false;
      }

      props.loadCsv(
        parsedText.meta.fields ?? [],
        parsedText.data.map((row: any, idx: number) => ({ ...row, uid: idx })),
      );
      return true;
    } catch (error) {
      showNotification({
        type: NotificationType.Error,
        message: t("error_reading_file_message", { error }),
      });
      return false;
    }
  };

  return (
    <Stack gap="0.5rem" w={600} maw="100%" mx="auto">
      <LoadingOverlay visible={isPending} />
      <FileInput
        {...fileField.getInputProps()}
        accept="text/csv"
        placeholder={<DimmedText size="sm">{t("select_csv_file")}</DimmedText>}
        elevation={0}
      />
      <Group align="center" w="100%" wrap="wrap" gap="0.5rem">
        <Switch
          label={
            <PrimaryText size="sm" style={{ whiteSpace: "nowrap" }}>
              {t("use_custom_delimiter")}
            </PrimaryText>
          }
          checked={useDelimiter.getValue()}
          onChange={(event) =>
            useDelimiter.setValue(event.currentTarget.checked)
          }
        />
        {useDelimiter.getValue() && (
          <TextInput
            {...delimiterField.getInputProps()}
            maxLength={1}
            minLength={1}
            elevation={0}
          />
        )}
      </Group>
      <Button
        variant="filled"
        color="primary"
        size="compact-sm"
        disabled={
          !fileField.getValue() ||
          !!fileField.error ||
          !!delimiterField.error ||
          isPending
        }
        onClick={async () => {
          startTransition(async () => {
            const file = fileField.getValue();
            const delimiter = useDelimiter.getValue()
              ? delimiterField.getValue()
              : null;

            if (!file || fileField.error) {
              showNotification({
                type: NotificationType.Error,
                message: t("please_select_valid_csv_file_message"),
              });
              return;
            }

            if (useDelimiter.getValue() && delimiterField.error) {
              showNotification({
                type: NotificationType.Error,
                message: t("please_provide_valid_delimiter_message"),
              });
              return;
            }

            if (file && !fileField.error) {
              const isSuccess = await loadCsv(file, delimiter);
              if (isSuccess) {
                props.launchNextDialog();
              }
            }
          });
        }}
      >
        {t("load_csv")}
      </Button>
    </Stack>
  );
};

export default LoadCsv;
