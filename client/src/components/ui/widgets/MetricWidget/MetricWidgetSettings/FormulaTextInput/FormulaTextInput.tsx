import { Combobox, TextInput, useCombobox } from "@mantine/core";
import React from "react";
import { PERIOD_KEYWORDS } from "~/helpers/metricWidget";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import dropdownClasses from "~/styles/Dropdown.module.css";

const SOURCE_METRICS: Record<string, string[]> = {
  transactions: ["sum", "count", "avg"],
  budgets: ["total", "spent", "remaining", "percent_used"],
  goals: [
    "percent_complete",
    "current_amount",
    "target",
    "monthly_contribution",
  ],
  accounts: ["balance"],
};

const SOURCES = Object.keys(SOURCE_METRICS);

const SOURCE_USES_PERIOD: Record<string, boolean> = {
  transactions: true,
  budgets: true,
  goals: false,
  accounts: false,
};

const SOURCE_PARAM_KEYS: Record<string, string[]> = {
  transactions: ["type", "category"],
  budgets: ["category"],
  goals: ["name"],
  accounts: ["type", "name"],
};

// Only sources/keys with known fixed enum values
const SOURCE_PARAM_VALUES: Record<string, Record<string, string[]>> = {
  transactions: {
    type: ["expense", "income", "all"],
  },
};

interface FormulaSuggestionData {
  transactionCategories: string[];
  budgetCategories: string[];
  goalNames: string[];
  accountNames: string[];
}

function getSourceParamValues(
  suggestionData: FormulaSuggestionData,
): Record<string, Record<string, string[]>> {
  return {
    transactions: {
      ...SOURCE_PARAM_VALUES.transactions,
      category: suggestionData.transactionCategories,
    },
    budgets: {
      category: suggestionData.budgetCategories,
    },
    goals: {
      name: suggestionData.goalNames,
    },
    accounts: {
      name: suggestionData.accountNames,
    },
  };
}

type FormulaStage =
  | { kind: "source"; query: string }
  | { kind: "metric"; source: string; query: string }
  | { kind: "first_arg"; source: string; metric: string; query: string }
  | {
      kind: "param_key";
      source: string;
      metric: string;
      argsPrefix: string;
      query: string;
    }
  | {
      kind: "param_value";
      source: string;
      metric: string;
      argsPrefix: string;
      key: string;
      query: string;
    };

interface FormulaSuggestion {
  label: string;
  insert: string;
}

interface ActiveFormulaMatch {
  start: number;
  end: number;
  query: string;
}

function parseFormulaStage(partial: string): FormulaStage | null {
  if (!partial.startsWith("@")) return null;

  const afterAt = partial.slice(1);
  const dotIdx = afterAt.indexOf(".");
  if (dotIdx === -1) {
    return { kind: "source", query: afterAt };
  }

  const source = afterAt.slice(0, dotIdx);
  const afterDot = afterAt.slice(dotIdx + 1);

  const parenIdx = afterDot.indexOf("(");
  if (parenIdx === -1) {
    return { kind: "metric", source, query: afterDot };
  }

  const metric = afterDot.slice(0, parenIdx);
  const afterParen = afterDot.slice(parenIdx + 1);

  if (!afterParen.includes(")")) {
    const commaIdx = afterParen.lastIndexOf(",");
    if (commaIdx === -1) {
      return { kind: "first_arg", source, metric, query: afterParen };
    }
    const argsPrefix = afterParen.slice(0, commaIdx).trim();
    const afterComma = afterParen.slice(commaIdx + 1).trimStart();
    const eqIdx = afterComma.indexOf("=");
    if (eqIdx === -1) {
      return {
        kind: "param_key",
        source,
        metric,
        argsPrefix,
        query: afterComma,
      };
    }
    return {
      kind: "param_value",
      source,
      metric,
      argsPrefix,
      key: afterComma.slice(0, eqIdx),
      query: afterComma.slice(eqIdx + 1),
    };
  }

  return null;
}

function getFormulaSuggestions(
  stage: FormulaStage | null,
  sourceParamValues: Record<string, Record<string, string[]>>,
): FormulaSuggestion[] {
  if (!stage) return [];

  switch (stage.kind) {
    case "source":
      return SOURCES.filter((s) => s.startsWith(stage.query.toLowerCase())).map(
        (s) => ({ label: s, insert: `@${s}.` }),
      );

    case "metric": {
      const metrics = SOURCE_METRICS[stage.source] ?? [];
      return metrics
        .filter((m) => m.startsWith(stage.query.toLowerCase()))
        .map((m) => ({ label: m, insert: `@${stage.source}.${m}(` }));
    }

    case "first_arg": {
      const usesPeriod = SOURCE_USES_PERIOD[stage.source] ?? false;
      if (usesPeriod) {
        const exactMatch = PERIOD_KEYWORDS.find((p) => p === stage.query);
        if (exactMatch) {
          const base = `@${stage.source}.${stage.metric}(${stage.query}`;
          return [
            { label: ")", insert: `${base})` },
            ...(SOURCE_PARAM_KEYS[stage.source] ?? []).map((k) => ({
              label: `${k}=…`,
              insert: `${base}, ${k}=`,
            })),
          ];
        }
        return PERIOD_KEYWORDS.filter((p) => p.startsWith(stage.query)).map(
          (p) => ({
            label: p,
            insert: `@${stage.source}.${stage.metric}(${p}`,
          }),
        );
      }
      // No period: first arg is a param key=value
      const eqIdx = stage.query.indexOf("=");
      if (eqIdx === -1) {
        return (SOURCE_PARAM_KEYS[stage.source] ?? [])
          .filter((k) => k.startsWith(stage.query.toLowerCase()))
          .map((k) => ({
            label: `${k}=…`,
            insert: `@${stage.source}.${stage.metric}(${k}=`,
          }));
      }
      const key = stage.query.slice(0, eqIdx);
      const valueQuery = stage.query.slice(eqIdx + 1);
      const values = sourceParamValues[stage.source]?.[key] ?? [];
      const exactValueMatch = values.find(
        (v) => v.toLowerCase() === valueQuery.toLowerCase(),
      );
      if (exactValueMatch) {
        return [
          {
            label: ")",
            insert: `@${stage.source}.${stage.metric}(${key}=${exactValueMatch})`,
          },
        ];
      }
      return values
        .filter((v) => v.toLowerCase().startsWith(valueQuery.toLowerCase()))
        .map((v) => ({
          label: v,
          insert: `@${stage.source}.${stage.metric}(${key}=${v}`,
        }));
    }

    case "param_key": {
      const usedKeys = stage.argsPrefix
        .split(",")
        .map((p) => (p.trim().split("=")[0] ?? "").trim())
        .filter(Boolean);
      const base = `@${stage.source}.${stage.metric}(${stage.argsPrefix}`;
      const suggestions: FormulaSuggestion[] = [];
      if (stage.query === "") {
        suggestions.push({ label: ")", insert: `${base})` });
      }
      suggestions.push(
        ...(SOURCE_PARAM_KEYS[stage.source] ?? [])
          .filter((k) => !usedKeys.includes(k))
          .filter((k) => k.startsWith(stage.query.toLowerCase()))
          .map((k) => ({ label: `${k}=…`, insert: `${base}, ${k}=` })),
      );
      return suggestions;
    }

    case "param_value": {
      const values = sourceParamValues[stage.source]?.[stage.key] ?? [];
      const usedKeys = stage.argsPrefix
        .split(",")
        .map((p) => (p.trim().split("=")[0] ?? "").trim())
        .filter(Boolean);
      const argsSoFar = stage.argsPrefix
        ? `${stage.argsPrefix}, ${stage.key}=`
        : `${stage.key}=`;
      const exactMatch = values.find(
        (v) => v.toLowerCase() === stage.query.toLowerCase(),
      );
      if (exactMatch) {
        const base = `@${stage.source}.${stage.metric}(${argsSoFar}${exactMatch}`;
        const remainingKeys = (SOURCE_PARAM_KEYS[stage.source] ?? []).filter(
          (k) => !usedKeys.includes(k) && k !== stage.key,
        );
        return [
          { label: ")", insert: `${base})` },
          ...remainingKeys.map((k) => ({
            label: `${k}=…`,
            insert: `${base}, ${k}=`,
          })),
        ];
      }
      return values
        .filter((v) => v.toLowerCase().startsWith(stage.query.toLowerCase()))
        .map((v) => ({
          label: v,
          insert: `@${stage.source}.${stage.metric}(${argsSoFar}${v}`,
        }));
    }
  }
}

function getActiveFormulaMatch(
  value: string,
  cursorPosition: number | null,
): ActiveFormulaMatch | null {
  if (cursorPosition === null) return null;

  const start = value.lastIndexOf("@", Math.max(cursorPosition - 1, 0));
  if (start === -1) return null;

  const closingBraceIndex = value.indexOf("}", start);
  if (closingBraceIndex !== -1 && closingBraceIndex < cursorPosition) {
    return null;
  }

  return {
    start,
    end: closingBraceIndex === -1 ? cursorPosition : closingBraceIndex + 1,
    query: value.slice(start, cursorPosition),
  };
}

export interface FormulaTextInputProps {
  label: React.ReactNode;
  placeholder: string;
  value: string;
  onChange: (value: string) => void;
  transactionCategories?: string[];
  budgetCategories?: string[];
  goalNames?: string[];
  accountNames?: string[];
}

const FormulaTextInput = ({
  label,
  placeholder,
  value,
  onChange,
  transactionCategories = [],
  budgetCategories = [],
  goalNames = [],
  accountNames = [],
}: FormulaTextInputProps): React.ReactNode => {
  const combobox = useCombobox({
    onDropdownClose: () => {
      combobox.resetSelectedOption();
    },
  });
  const inputRef = React.useRef<HTMLInputElement>(null);
  const [activeFormula, setActiveFormula] =
    React.useState<ActiveFormulaMatch | null>(null);

  const sourceParamValues = React.useMemo(
    () =>
      getSourceParamValues({
        transactionCategories,
        budgetCategories,
        goalNames,
        accountNames,
      }),
    [transactionCategories, budgetCategories, goalNames, accountNames],
  );

  const filteredSuggestions = React.useMemo((): FormulaSuggestion[] => {
    if (!activeFormula) return [];
    const stage = parseFormulaStage(activeFormula.query);
    return getFormulaSuggestions(stage, sourceParamValues);
  }, [activeFormula, sourceParamValues]);

  const syncActiveFormula = React.useCallback(
    (nextValue: string) => {
      const cursorPosition =
        inputRef.current?.selectionStart ?? nextValue.length;
      const match = getActiveFormulaMatch(nextValue, cursorPosition);

      setActiveFormula(match);

      if (match) {
        const stage = parseFormulaStage(match.query);
        if (getFormulaSuggestions(stage, sourceParamValues).length > 0) {
          combobox.openDropdown();
          combobox.updateSelectedOptionIndex();
          return;
        }
      }

      combobox.closeDropdown();
    },
    [combobox, sourceParamValues],
  );

  React.useEffect(() => {
    if (filteredSuggestions.length === 0) {
      combobox.closeDropdown();
    }
  }, [combobox, filteredSuggestions.length]);

  const handleOptionSubmit = (suggestion: string) => {
    const match =
      getActiveFormulaMatch(
        value,
        inputRef.current?.selectionStart ?? value.length,
      ) ?? activeFormula;

    if (!match) return;

    const nextValue =
      value.slice(0, match.start) + suggestion + value.slice(match.end);

    onChange(nextValue);

    requestAnimationFrame(() => {
      inputRef.current?.focus();
      const nextCursorPosition = match.start + suggestion.length;
      inputRef.current?.setSelectionRange(
        nextCursorPosition,
        nextCursorPosition,
      );
      syncActiveFormula(nextValue);
    });
  };

  return (
    <Combobox
      classNames={{
        dropdown: dropdownClasses.dropdown,
      }}
      onOptionSubmit={handleOptionSubmit}
      store={combobox}
    >
      <Combobox.Target>
        <TextInput
          inputWrapperOrder={["label", "input"]}
          label={label}
          placeholder={placeholder}
          ref={inputRef}
          styles={{
            input: { fontFamily: "monospace", fontSize: "0.85rem" },
          }}
          value={value}
          onChange={(event) => {
            const nextValue = event.currentTarget.value;
            onChange(nextValue);
            syncActiveFormula(nextValue);
          }}
          onClick={(event) => syncActiveFormula(event.currentTarget.value)}
          onFocus={(event) => syncActiveFormula(event.currentTarget.value)}
          onKeyUp={(event) => {
            if (
              event.key === "ArrowDown" ||
              event.key === "ArrowUp" ||
              event.key === "Enter" ||
              event.key === "Escape"
            ) {
              return;
            }

            syncActiveFormula(event.currentTarget.value);
          }}
          onBlur={() => {
            window.setTimeout(() => {
              combobox.closeDropdown();
            }, 0);
          }}
          onKeyDown={(event) => {
            // Prevent the cursor from jumping to start/end of the input on arrow
            // key press. Mantine's Combobox.Target already fires selectNextOption /
            // selectPreviousOption / clickSelectedOption / closeDropdown internally,
            // so we must NOT call those ourselves or every nav step fires twice.
            if (event.key === "ArrowDown" || event.key === "ArrowUp") {
              event.preventDefault();
            }

            // Open the dropdown on ArrowDown when it is currently closed —
            // Mantine's internal handler only navigates when the dropdown is open.
            if (
              !combobox.dropdownOpened &&
              event.key === "ArrowDown" &&
              filteredSuggestions.length > 0
            ) {
              combobox.openDropdown();
              combobox.selectFirstOption();
            }
          }}
        />
      </Combobox.Target>

      {filteredSuggestions.length > 0 && (
        <Combobox.Dropdown>
          <Combobox.Options mah={240} style={{ overflowY: "auto" }}>
            {filteredSuggestions.map((suggestion) => (
              <Combobox.Option
                key={suggestion.insert}
                value={suggestion.insert}
              >
                <PrimaryText
                  size="sm"
                  style={{
                    fontFamily: "monospace",
                    overflowWrap: "anywhere",
                  }}
                >
                  {suggestion.label}
                </PrimaryText>
              </Combobox.Option>
            ))}
          </Combobox.Options>
        </Combobox.Dropdown>
      )}
    </Combobox>
  );
};

export default FormulaTextInput;
