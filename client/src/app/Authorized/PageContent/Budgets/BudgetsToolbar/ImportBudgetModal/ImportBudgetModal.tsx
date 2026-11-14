import { Modal, Stepper } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { useDisclosure } from "@mantine/hooks";
import { FileUpIcon } from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useImportBudgetMutation } from "~/hooks/mutations/budgets/useImportBudgetMutation";
import {
  IBudgetImportCategoryCreateRequest,
  IBudgetImportItemRequest,
  IBudgetImportRequest,
  IBudgetTemplateCategoryItem,
  IBudgetTemplateExport,
  IMissingCategoryResolution,
} from "~/models/budgetTemplate";
import { useLocale } from "~/providers/LocaleProvider/LocaleProvider";
import CategoryMapping from "./CategoryMapping/CategoryMapping";
import ImportCompleted from "./ImportCompleted/ImportCompleted";
import LoadJson from "./LoadJson/LoadJson";
import PreviewBudget from "./PreviewBudget/PreviewBudget";

interface ImportBudgetModalProps {
  currentDate: Date;
  opened?: boolean;
  onOpen?: () => void;
  onClose?: () => void;
  showTrigger?: boolean;
}

const ImportBudgetModal = ({
  currentDate,
  opened: externalOpened,
  onOpen,
  onClose,
  showTrigger = false,
}: ImportBudgetModalProps): React.ReactNode => {
  const [internalOpened, { open: openInternal, close: closeInternal }] =
    useDisclosure(false);
  const isControlled = externalOpened !== undefined;
  const isOpened = isControlled ? externalOpened : internalOpened;
  const [activeStep, setActiveStep] = React.useState(0);

  const { t } = useTranslation();
  const { dayjs } = useLocale();
  const importBudgetMutation = useImportBudgetMutation();

  const [loadedTemplate, setLoadedTemplate] =
    React.useState<IBudgetTemplateExport | null>(null);
  const [targetMonth, setTargetMonth] = React.useState<Date>(currentDate);
  const [overwriteExisting, setOverwriteExisting] = React.useState<boolean>(true);
  const [resolutions, setResolutions] = React.useState<
    Map<string, IMissingCategoryResolution>
  >(new Map());
  const [importedResult, setImportedResult] = React.useState<{
    itemsCount: number;
    newCategoriesCount: number;
  }>({ itemsCount: 0, newCategoriesCount: 0 });

  React.useEffect(() => {
    setTargetMonth(currentDate);
  }, [currentDate]);

  const resetState = () => {
    setActiveStep(0);
    setLoadedTemplate(null);
    setTargetMonth(currentDate);
    setOverwriteExisting(true);
    setResolutions(new Map());
    setImportedResult({ itemsCount: 0, newCategoriesCount: 0 });
  };

  const handleOpen = () => {
    resetState();
    if (isControlled) {
      onOpen?.();
    } else {
      openInternal();
    }
  };

  const handleClose = () => {
    resetState();
    if (isControlled) {
      onClose?.();
    } else {
      closeInternal();
    }
  };

  const handleTemplateLoaded = (template: IBudgetTemplateExport) => {
    setLoadedTemplate(template);
    setResolutions(new Map());
  };

  const handleSubmitImport = () => {
    if (!loadedTemplate) {
      return;
    }

    const newCategories: IBudgetImportCategoryCreateRequest[] = [];
    const items: IBudgetImportItemRequest[] = [];

    // Collect new categories to create
    resolutions.forEach((res) => {
      if (res.action === "create") {
        newCategories.push({
          name: res.categoryName,
          parent: res.parentName,
          categoryType: res.categoryType,
          icon: res.icon,
        });
      }
    });

    const processCategory = (cat: IBudgetTemplateCategoryItem) => {
      const lower = cat.name.toLowerCase();
      const resolution = resolutions.get(lower);

      if (resolution && resolution.action === "skip") {
        // Skip this category and its budget limit
      } else {
        const effectiveCategoryName =
          resolution && resolution.action === "map" && resolution.mappedCategory
            ? resolution.mappedCategory
            : cat.name;

        const effectiveIcon =
          resolution && resolution.action === "map" ? null : (cat.icon ?? null);

        items.push({
          category: effectiveCategoryName,
          limit: cat.limit,
          rolloverEnabled: cat.rolloverEnabled,
          icon: effectiveIcon,
        });
      }

      cat.subcategories?.forEach(processCategory);
    };

    loadedTemplate.categories.forEach(processCategory);

    const payload: IBudgetImportRequest = {
      month: dayjs(targetMonth).format("YYYY-MM-01"),
      overwriteExisting,
      newCategories,
      items,
    };

    importBudgetMutation.mutate(payload, {
      onSuccess: () => {
        setImportedResult({
          itemsCount: items.length,
          newCategoriesCount: newCategories.length,
        });
        setActiveStep(3);
      },
    });
  };

  return (
    <>
      {showTrigger && (
        <Button
          variant="filled"
          color="primary"
          size="xs"
          rightSection={<FileUpIcon size="1rem" />}
          onClick={handleOpen}
        >
          {t("import_budget")}
        </Button>
      )}

      <Modal
        opened={isOpened}
        onClose={handleClose}
        size="lg"
        title={t("import_budget")}
      >
        <Stepper active={activeStep} allowNextStepsSelect={false} w="100%">
          <Stepper.Step
            label={t("step_1")}
            description={t("budget_import_load_json")}
          >
            <LoadJson
              onTemplateLoaded={handleTemplateLoaded}
              launchNextDialog={() => setActiveStep(1)}
            />
          </Stepper.Step>

          <Stepper.Step
            label={t("step_2")}
            description={t("budget_import_preview")}
          >
            {loadedTemplate && (
              <PreviewBudget
                template={loadedTemplate}
                targetMonth={targetMonth}
                setTargetMonth={setTargetMonth}
                overwriteExisting={overwriteExisting}
                setOverwriteExisting={setOverwriteExisting}
                goBack={() => setActiveStep(0)}
                advanceToNext={() => setActiveStep(2)}
              />
            )}
          </Stepper.Step>

          <Stepper.Step
            label={t("step_3")}
            description={t("import")}
          >
            {loadedTemplate && (
              <CategoryMapping
                template={loadedTemplate}
                resolutions={resolutions}
                setResolutions={setResolutions}
                goBack={() => setActiveStep(1)}
                onSubmit={handleSubmitImport}
                isSubmitting={importBudgetMutation.isPending}
              />
            )}
          </Stepper.Step>

          <Stepper.Completed>
            <ImportCompleted
              importedCount={importedResult.itemsCount}
              newCategoriesCount={importedResult.newCategoriesCount}
              targetMonth={targetMonth}
              closeModal={handleClose}
            />
          </Stepper.Completed>
        </Stepper>
      </Modal>
    </>
  );
};

export default ImportBudgetModal;
