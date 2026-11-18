import { IBudgetTemplateExport } from "~/models/budgetTemplate";

export const downloadBudgetTemplateJson = (
  template: IBudgetTemplateExport,
  monthString: string,
) => {
  const jsonString = JSON.stringify(template, null, 2);
  const blob = new Blob([jsonString], { type: "application/json" });
  const url = URL.createObjectURL(blob);

  const link = document.createElement("a");
  link.href = url;
  link.download = `budget-${monthString}.json`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
};
