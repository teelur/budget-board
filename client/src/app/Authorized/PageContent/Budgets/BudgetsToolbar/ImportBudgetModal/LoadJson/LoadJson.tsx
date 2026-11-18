import { Group, Stack } from "@mantine/core";
import { Alert, BodyText, Button, FileInput } from "@teelur/budget-board-ui";
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
      setErrors([t("budget_import_file_too_large")]);
      return;
    }

    try {
      const text = await file.text();
      const result = validateBudgetTemplateJson(text, t);

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
    <Stack gap="0.5rem" maw="100%" mx="auto">
      <BodyText tone="secondary" size="sm">
        {t("budget_import_load_description")}
      </BodyText>
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
          color="error"
          variant="light"
          icon={<AlertCircleIcon size={16} />}
          title={t("budget_import_validation_failed")}
        >
          <Stack gap="0.25rem">
            {errors.map((error, idx) => (
              <BodyText key={idx} tone="primary" size="xs">
                • {error}
              </BodyText>
            ))}
          </Stack>
        </Alert>
      )}

      {warnings.length > 0 && (
        <Alert
          color="warning"
          variant="light"
          icon={<AlertCircleIcon size={16} />}
          title={t("budget_import_validation_warnings")}
        >
          <Stack gap="0.25rem">
            {warnings.map((warn, idx) => (
              <BodyText key={idx} tone="primary" size="xs">
                • {warn}
              </BodyText>
            ))}
          </Stack>
        </Alert>
      )}

      {loadedTemplate && (
        <Alert
          color="success"
          variant="light"
          icon={<CheckCircle2Icon size={16} />}
          title={t("budget_import_validation_passed")}
        >
          <BodyText tone="primary" size="xs">
            {t("budget_import_valid_summary", {
              name: loadedTemplate.name,
              count: loadedTemplate.categories.length,
            })}
          </BodyText>
        </Alert>
      )}
      <Group w="100%">
        <Button
          variant="filled"
          color="primary"
          size="compact-sm"
          flex="1 1 auto"
          disabled={!loadedTemplate}
          onClick={handleNext}
        >
          {t("budget_import_load_json")}
        </Button>
      </Group>
    </Stack>
  );
};

export default LoadJson;
