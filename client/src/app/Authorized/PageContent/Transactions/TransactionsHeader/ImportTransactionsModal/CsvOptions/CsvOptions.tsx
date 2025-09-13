import {
  Button,
  Divider,
  FileInput,
  Group,
  Stack,
  Switch,
  Text,
  TextInput,
} from "@mantine/core";
import { useField } from "@mantine/form";
import React from "react";

interface CsvOptionsProps {
  loadCsv: (file: File, delimiter: string) => void;
}

const CsvOptions = (props: CsvOptionsProps): React.ReactNode => {
  const fileField = useField<File | null>({
    initialValue: null,
    validateOnBlur: true,
    validate: (value) => {
      if (!value) {
        return;
      }
      if (value.type !== "text/csv") {
        return "File must be a CSV file";
      }
      return null;
    },
  });
  const delimiterField = useField<string>({
    initialValue: ",",
    validateOnBlur: true,
    validate: (value) => {
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

  return (
    <Stack gap="0.5rem">
      <Divider label="CSV Options" labelPosition="center" />
      <Stack gap="0.5rem">
        <FileInput
          {...fileField.getInputProps()}
          accept="text/csv"
          placeholder={
            <Text fw={600} size="sm" c="dimmed">
              Select CSV file
            </Text>
          }
          flex="1 1 auto"
          miw={200}
        />
        <Group align="center" w="100%" wrap="nowrap" gap="0.5rem">
          <Switch
            label={
              <Text fw={600} size="sm" style={{ whiteSpace: "nowrap" }}>
                Specify delimiter
              </Text>
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
            />
          )}
        </Group>
        <Button
          onClick={() => {
            const file = fileField.getValue();
            const delimiter = delimiterField.getValue();
            if (
              file &&
              delimiter &&
              !fileField.error &&
              !delimiterField.error
            ) {
              props.loadCsv(file, delimiter);
            }
          }}
          disabled={
            !fileField.getValue() || !!fileField.error || !!delimiterField.error
          }
        >
          Load CSV
        </Button>
      </Stack>
    </Stack>
  );
};

export default CsvOptions;
