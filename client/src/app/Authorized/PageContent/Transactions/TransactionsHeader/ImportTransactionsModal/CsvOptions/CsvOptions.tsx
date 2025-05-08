import { Divider, FileInput, Group, Stack, TextInput } from "@mantine/core";
import React from "react";

interface CsvOptionsProps {
  fileField: File | null;
  setFileField: (file: File | null) => void;
  delimiterField: string;
  setDelimiterField: (delimiter: string) => void;
  handleFileChange: () => void;
}

const CsvOptions = (props: CsvOptionsProps): React.ReactNode => {
  return (
    <Stack gap={0}>
      <Divider label="CSV Options" labelPosition="center" />
      <Group w="100%" wrap="nowrap">
        <FileInput
          value={props.fileField}
          clearable
          accept="text/csv"
          placeholder="Upload .csv"
          label="CSV File"
          onChange={(value) => {
            props.setFileField(value);
            props.handleFileChange();
          }}
          w="100%"
          miw={180}
        />
        <TextInput
          value={props.delimiterField}
          label="Delimiter"
          onChange={(event) => {
            props.setDelimiterField(event.currentTarget.value);
            props.handleFileChange();
          }}
          maw={70}
        />
      </Group>
    </Stack>
  );
};

export default CsvOptions;
