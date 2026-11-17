import { Alert, FileInput, Group, Stack, Text } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { AlertCircleIcon, CheckCircle2Icon, FileJsonIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { validateBudgetTemplateJson } from "~/helpers/budgetValidation";
import { IBudgetTemplateExport } from "~/models/budgetTemplate";

interface LoadJsonProps {
  onTemplateLoaded: (template: IBudgetTemplateExport) => void;
  launchNextDialog: () => void;
}

const LoadJson = ({
  onTemplateLoaded,
  launchNextDialog,
}: LoadJsonProps): React.ReactNode => {
  const { t } = useTranslation();
  const [selectedFile, setSelectedFile] = React.useState<File | null>(null);
  const [errors, setErrors] = React.useState<string[]>([]);
  const [warnings, setWarnings] = React.useState<string[]>([]);
  const [loadedTemplate, setLoadedTemplate] =
    React.useState<IBudgetTemplateExport | null>(null);

  const handleFileChange = async (file: File | null) => {
    setSelectedFile(file);
    setErrors([]);
    setWarnings([]);
    setLoadedTemplate(null);

    if (!file) {
      return;
    }

    if (!file.name.toLowerCase().endsWith(".json")) {
      setErrors([t("budget_import_must_be_json")]);
      return;
    }

    const maxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    if (file.size > maxFileSizeBytes) {
      setErrors(["File size exceeds the 5 MB limit."]);
      return;
    }

    try {
      const text = await file.text();
      const result = validateBudgetTemplateJson(text);

      if (!result.isValid || !result.template) {
        setErrors(result.errors);
        setWarnings(result.warnings);
      } else {
        setWarnings(result.warnings);
        setLoadedTemplate(result.template);
        onTemplateLoaded(result.template);
      }
    } catch {
      setErrors([t("budget_import_read_error")]);
    }
  };

  const handleNext = () => {
    if (loadedTemplate) {
      launchNextDialog();
    }
  };

  return (
    <Stack gap="1rem" w={700} maw="100%" mx="auto" pt="0.5rem">
      <Text size="sm" c="dimmed">
        {t("budget_import_load_description")}
      </Text>

      <FileInput
        label={t("budget_import_select_file")}
        placeholder={t("budget_import_file_placeholder")}
        leftSection={<FileJsonIcon size={16} />}
        accept=".json,application/json"
        value={selectedFile}
        onChange={handleFileChange}
        clearable
      />

      {errors.length > 0 && (
        <Alert
          icon={<AlertCircleIcon size={16} />}
          title={t("budget_import_validation_failed")}
          color="red"
          variant="light"
        >
          <Stack gap="0.25rem">
            {errors.map((error, idx) => (
              <Text key={idx} size="xs">
                • {error}
              </Text>
            ))}
          </Stack>
        </Alert>
      )}

      {warnings.length > 0 && (
        <Alert
          icon={<AlertCircleIcon size={16} />}
          title={t("warning")}
          color="yellow"
          variant="light"
        >
          <Stack gap="0.25rem">
            {warnings.map((warn, idx) => (
              <Text key={idx} size="xs">
                • {warn}
              </Text>
            ))}
          </Stack>
        </Alert>
      )}

      {loadedTemplate && (
        <Alert
          icon={<CheckCircle2Icon size={16} />}
          title={t("budget_import_validation_passed")}
          color="green"
          variant="light"
        >
          <Text size="xs">
            {t("budget_import_valid_summary", {
              name: loadedTemplate.name,
              count: loadedTemplate.categories.length,
            })}
          </Text>
        </Alert>
      )}

      <Group justify="flex-end" mt="1rem">
        <Button
          variant="filled"
          color="primary"
          disabled={!loadedTemplate}
          onClick={handleNext}
        >
          {t("next")}
        </Button>
      </Group>
    </Stack>
  );
};

export default LoadJson;
