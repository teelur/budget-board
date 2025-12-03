import { Button, Group, LoadingOverlay, Stack, Switch } from "@mantine/core";
import { useField } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import Papa from "papaparse";
import React from "react";
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

  const fileField = useField<File | null>({
    initialValue: null,
    validateOnBlur: true,
    validate: (value) => {
      if (!value) {
        return;
      }

      const nameIsCsv = value.name?.toLowerCase().endsWith(".csv");
      if (!nameIsCsv) {
        return `File must be a CSV file. Found type: ${
          value.type || "unknown"
        }; filename: ${value.name}`;
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
        return "Delimiter is required";
      }
      if (value.length > 1) {
        return "Delimiter must be a single character";
      }
      return null;
    },
  });
  const useDelimiter = useField<boolean>({
    initialValue: false,
  });

  const loadCsv = async (
    file: File,
    delimiter: string | null
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
          new Set(parsedText.errors.map((error) => error.message))
        );

        uniqueErrorMessages.forEach((errorMessage) => {
          notifications.show({
            color: "red",
            message: `Error parsing CSV: ${errorMessage}`,
          });
        });
        return false;
      }

      if (parsedText.data.length === 0) {
        notifications.show({
          color: "red",
          message: "CSV file is empty",
        });
        return false;
      }

      props.loadCsv(
        parsedText.meta.fields ?? [],
        parsedText.data.map((row: any, idx: number) => ({ ...row, uid: idx }))
      );
      return true;
    } catch (error) {
      notifications.show({
        color: "red",
        message: `Error reading file: ${error}`,
      });
      return false;
    }
  };

  return (
    <Stack gap="0.5rem" w={600} maw="100%">
      <LoadingOverlay visible={isPending} />
      <FileInput
        {...fileField.getInputProps()}
        accept="text/csv"
        placeholder={<DimmedText size="sm">Select a CSV file</DimmedText>}
        elevation={0}
      />
      <Group align="center" w="100%" wrap="wrap" gap="0.5rem">
        <Switch
          label={
            <PrimaryText size="sm" style={{ whiteSpace: "nowrap" }}>
              Specify delimiter
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
        onClick={async () => {
          startTransition(async () => {
            const file = fileField.getValue();
            const delimiter = useDelimiter.getValue()
              ? delimiterField.getValue()
              : null;

            if (!file || fileField.error) {
              notifications.show({
                color: "red",
                message: "Please provide a valid CSV file",
              });
              return;
            }

            if (useDelimiter.getValue() && delimiterField.error) {
              notifications.show({
                color: "red",
                message: "Please provide a valid delimiter",
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
        disabled={
          !fileField.getValue() ||
          !!fileField.error ||
          !!delimiterField.error ||
          isPending
        }
      >
        Load CSV
      </Button>
    </Stack>
  );
};

export default LoadCsv;
