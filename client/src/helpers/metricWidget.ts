import dayjs from "~/shared/dayjs";
import { ITransaction } from "~/models/transaction";
import { IBudget } from "~/models/budget";
import { IGoalResponse } from "~/models/goal";
import { IAccountResponse } from "~/models/account";
import { IAccountType } from "~/models/accountType";
import {
  ExpressionToken,
  MetricDataRequirements,
  MetricToken,
} from "~/models/metricWidget";
import { convertNumberToCurrency, SignDisplay } from "~/helpers/currency";
import { getGoalTargetAmount } from "~/helpers/goals";
import { filterVisibleAccounts, getAccountsOfTypes } from "~/helpers/accounts";
import { getVisibleTransactions } from "~/helpers/transactions";
import { areStringsEqual } from "~/helpers/utils";

export const PERIOD_KEYWORDS = [
  "this_month",
  "last_month",
  "this_year",
  "last_3_months",
  "last_6_months",
  "last_12_months",
  "all_time",
] as const;

export type PeriodKeyword = (typeof PERIOD_KEYWORDS)[number];

const EXPRESSION_REGEX = /@(\w+)\.(\w+)\(([^)]*)\)/g;

type MetricFormat = "currency" | "percent" | "integer" | "decimal" | "number";

/**
 * Default metric format inference based on the source and metric name.
 * Formats are always inferred and cannot be overridden.
 */
const DEFAULT_METRIC_FORMATS: Record<string, MetricFormat> = {
  "transactions.sum": "currency",
  "transactions.count": "integer",
  "transactions.avg": "currency",
  "budgets.total": "currency",
  "budgets.spent": "currency",
  "budgets.remaining": "currency",
  "budgets.percent_used": "percent",
  "goals.percent_complete": "percent",
  "goals.current_amount": "currency",
  "goals.target": "currency",
  "goals.monthly_contribution": "currency",
  "accounts.balance": "currency",
};

function parseArgs(argsStr: string): {
  period?: string;
  params: Record<string, string>;
} {
  const parts = argsStr
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean);
  let period: string | undefined;
  const params: Record<string, string> = {};

  parts.forEach((part, index) => {
    if (index === 0 && !part.includes("=")) {
      if ((PERIOD_KEYWORDS as readonly string[]).includes(part)) {
        period = part;
      }
    } else if (part.includes("=")) {
      const eqIdx = part.indexOf("=");
      const key = part.slice(0, eqIdx).trim();
      const value = part.slice(eqIdx + 1).trim();
      params[key] = value;
    }
  });

  return { period, params };
}

export function parseTemplate(template: string): MetricToken[] {
  const tokens: MetricToken[] = [];
  let lastIndex = 0;
  const regex = new RegExp(EXPRESSION_REGEX.source, "g");
  let match: RegExpExecArray | null;

  while ((match = regex.exec(template)) !== null) {
    if (match.index > lastIndex) {
      tokens.push({
        type: "literal",
        text: template.slice(lastIndex, match.index),
      });
    }

    const [raw, source, metric, argsStr] = match as RegExpExecArray & string[];
    const { period, params } = parseArgs(argsStr ?? "");

    tokens.push({
      type: "expression",
      source,
      metric,
      period,
      params,
      raw,
    } as ExpressionToken);

    lastIndex = match.index + raw.length;
  }

  if (lastIndex < template.length) {
    tokens.push({ type: "literal", text: template.slice(lastIndex) });
  }

  return tokens;
}

export function getMonthsForPeriod(period: string): Date[] {
  const now = dayjs();
  switch (period) {
    case "this_month":
      return [now.startOf("month").toDate()];
    case "last_month":
      return [now.subtract(1, "month").startOf("month").toDate()];
    case "this_year": {
      const months: Date[] = [];
      for (let m = 0; m <= now.month(); m++) {
        months.push(now.year(now.year()).month(m).startOf("month").toDate());
      }
      return months;
    }
    case "last_3_months":
      return Array.from({ length: 3 }, (_, i) =>
        now.subtract(i, "month").startOf("month").toDate(),
      );
    case "last_6_months":
      return Array.from({ length: 6 }, (_, i) =>
        now.subtract(i, "month").startOf("month").toDate(),
      );
    case "last_12_months":
      return Array.from({ length: 12 }, (_, i) =>
        now.subtract(i, "month").startOf("month").toDate(),
      );
    case "all_time":
      return [];
    default:
      return [now.startOf("month").toDate()];
  }
}

function isInPeriod(isoDateStr: string, period: string): boolean {
  // Compare using date-only values to avoid timezone-driven month boundary shifts.
  const normalizedDateStr = isoDateStr.includes("T")
    ? isoDateStr.slice(0, 10)
    : isoDateStr;

  const d = dayjs(normalizedDateStr);
  if (!d.isValid()) return false;

  const now = dayjs();

  switch (period) {
    case "this_month":
      return d.isSame(now, "month");
    case "last_month":
      return d.isSame(now.subtract(1, "month"), "month");
    case "this_year":
      return d.isSame(now, "year");
    case "last_3_months":
      return (
        d.isAfter(
          now.subtract(3, "month").startOf("month").subtract(1, "day"),
        ) && !d.isAfter(now.endOf("month"))
      );
    case "last_6_months":
      return (
        d.isAfter(
          now.subtract(6, "month").startOf("month").subtract(1, "day"),
        ) && !d.isAfter(now.endOf("month"))
      );
    case "last_12_months":
      return (
        d.isAfter(
          now.subtract(12, "month").startOf("month").subtract(1, "day"),
        ) && !d.isAfter(now.endOf("month"))
      );
    case "all_time":
      return true;
    default:
      return d.isSame(now, "month");
  }
}

export interface MetricDataContext {
  transactions: ITransaction[];
  budgets: IBudget[];
  goals: IGoalResponse[];
  accounts: IAccountResponse[];
  accountTypes: IAccountType[];
  preferredCurrency: string;
  intlLocale: string;
}

function formatValue(
  value: number,
  format: MetricFormat,
  currency: string,
  locale: string,
): string {
  switch (format) {
    case "currency":
      return convertNumberToCurrency(
        value,
        true,
        currency,
        SignDisplay.Auto,
        locale,
      );
    case "percent":
      return `${Math.round(value * 10) / 10}%`;
    case "integer":
      return new Intl.NumberFormat(locale, {
        maximumFractionDigits: 0,
      }).format(Math.round(value));
    case "decimal":
      return new Intl.NumberFormat(locale, {
        maximumFractionDigits: 2,
        minimumFractionDigits: 2,
      }).format(value);
    case "number":
      return new Intl.NumberFormat(locale, {
        maximumFractionDigits: 2,
      }).format(value);
    default:
      return String(value);
  }
}

function getMetricFormat(token: ExpressionToken): MetricFormat {
  // Format is always inferred from source.metric; user-specified format overrides are not supported.
  return DEFAULT_METRIC_FORMATS[`${token.source}.${token.metric}`] ?? "number";
}

function resolveTransactions(
  metric: string,
  period: string,
  params: Record<string, string>,
  ctx: MetricDataContext,
): number {
  const type = params["type"] ?? "all";
  const category = params["category"];

  let txs = getVisibleTransactions(ctx.transactions).filter((t) =>
    isInPeriod(t.date, period),
  );

  if (type === "expense") {
    txs = txs.filter((t) => !areStringsEqual(t.category ?? "", "Income"));
  } else if (type === "income") {
    txs = txs.filter((t) => areStringsEqual(t.category ?? "", "Income"));
  }

  if (category) {
    txs = txs.filter(
      (t) =>
        areStringsEqual(t.category ?? "", category) ||
        areStringsEqual(t.subcategory ?? "", category),
    );
  }

  switch (metric) {
    case "sum": {
      const sum = txs.reduce((n, t) => n + t.amount, 0);
      return type === "expense" ? Math.abs(sum) : sum;
    }
    case "count":
      return txs.length;
    case "avg": {
      if (txs.length === 0) return 0;
      const avg = txs.reduce((n, t) => n + t.amount, 0) / txs.length;
      return type === "expense" ? Math.abs(avg) : avg;
    }
    default:
      return 0;
  }
}

function resolveBudgets(
  metric: string,
  period: string,
  params: Record<string, string>,
  ctx: MetricDataContext,
): number {
  const category = params["category"];
  if (!category) return 0;

  let budgets = ctx.budgets
    .filter((b) => isInPeriod(dayjs(b.month).format("YYYY-MM-DD"), period))
    .filter((b) => areStringsEqual(b.category, category));

  const total = budgets.reduce((n, b) => n + b.limit, 0);
  if (metric === "total") return total;

  let txs = getVisibleTransactions(ctx.transactions)
    .filter((t) => isInPeriod(t.date, period))
    .filter(
      (t) =>
        areStringsEqual(t.category ?? "", category) ||
        areStringsEqual(t.subcategory ?? "", category),
    );

  const spent = Math.abs(txs.reduce((n, t) => n + t.amount, 0));

  switch (metric) {
    case "spent":
      return spent;
    case "remaining":
      return Math.max(0, total - spent);
    case "percent_used":
      return total > 0 ? (spent / total) * 100 : 0;
    default:
      return 0;
  }
}

function resolveGoals(
  metric: string,
  params: Record<string, string>,
  ctx: MetricDataContext,
): number {
  const name = params["name"];
  if (!name) return 0;

  const goal = ctx.goals.find((g) => areStringsEqual(g.name, name));
  if (!goal) return 0;

  switch (metric) {
    case "percent_complete":
      return goal.percentComplete;
    case "target":
      return getGoalTargetAmount(goal.amount, goal.initialAmount);
    case "current_amount":
      return (
        goal.accounts.reduce((n, a) => n + a.currentBalance, 0) -
        goal.initialAmount
      );
    case "monthly_contribution":
      return goal.monthlyContribution;
    default:
      return 0;
  }
}

function resolveAccounts(
  metric: string,
  params: Record<string, string>,
  ctx: MetricDataContext,
): number {
  if (metric !== "balance") return 0;

  const visible = filterVisibleAccounts(ctx.accounts);
  const typeName = params["type"];
  const accountName = params["name"];

  if (typeName) {
    const matching = getAccountsOfTypes(visible, [typeName], ctx.accountTypes);
    return matching.reduce((n, a) => n + a.currentBalance, 0);
  }

  if (accountName) {
    const account = visible.find((a) => areStringsEqual(a.name, accountName));
    return account?.currentBalance ?? 0;
  }

  return 0;
}

function resolveExpression(
  token: ExpressionToken,
  ctx: MetricDataContext,
): string {
  try {
    const period = token.period ?? "this_month";
    let value: number;

    switch (token.source) {
      case "transactions":
        value = resolveTransactions(token.metric, period, token.params, ctx);
        break;
      case "budgets":
        value = resolveBudgets(token.metric, period, token.params, ctx);
        break;
      case "goals":
        value = resolveGoals(token.metric, token.params, ctx);
        break;
      case "accounts":
        value = resolveAccounts(token.metric, token.params, ctx);
        break;
      default:
        return "[unknown source]";
    }

    return formatValue(
      value,
      getMetricFormat(token),
      ctx.preferredCurrency,
      ctx.intlLocale,
    );
  } catch {
    return "[error]";
  }
}

export function resolveTemplate(
  tokens: MetricToken[],
  ctx: MetricDataContext,
): string {
  return tokens
    .map((token) => {
      if (token.type === "literal") return token.text;
      return resolveExpression(token, ctx);
    })
    .join("");
}

function getAllExpressionTokens(
  ...tokenGroups: MetricToken[][]
): ExpressionToken[] {
  return tokenGroups
    .flat()
    .filter((t): t is ExpressionToken => t.type === "expression");
}

export function buildDataRequirements(
  ...tokenGroups: MetricToken[][]
): MetricDataRequirements {
  const expressions = getAllExpressionTokens(...tokenGroups);

  const needsTransactions = expressions.some(
    (e) => e.source === "transactions" || e.source === "budgets",
  );
  const needsBudgets = expressions.some((e) => e.source === "budgets");
  const needsGoals = expressions.some((e) => e.source === "goals");
  const needsAccounts = expressions.some((e) => e.source === "accounts");

  const transactionPeriods = new Set<string>();
  const budgetPeriods = new Set<string>();

  expressions.forEach((e) => {
    const p = e.period ?? "this_month";
    if (e.source === "transactions" || e.source === "budgets") {
      transactionPeriods.add(p);
    }
    if (e.source === "budgets") {
      budgetPeriods.add(p);
    }
  });

  const needsAllTimeTransactions = transactionPeriods.has("all_time");

  const transactionMonthsMap = new Map<string, Date>();
  transactionPeriods.forEach((period) => {
    if (period === "all_time") return;
    getMonthsForPeriod(period).forEach((d) => {
      transactionMonthsMap.set(`${d.getFullYear()}-${d.getMonth()}`, d);
    });
  });

  const budgetMonthsMap = new Map<string, Date>();
  budgetPeriods.forEach((period) => {
    if (period === "all_time") return;
    getMonthsForPeriod(period).forEach((d) => {
      budgetMonthsMap.set(`${d.getFullYear()}-${d.getMonth()}`, d);
    });
  });

  return {
    needsTransactions,
    transactionMonths: Array.from(transactionMonthsMap.values()),
    needsAllTimeTransactions,
    needsBudgets,
    budgetMonths: Array.from(budgetMonthsMap.values()),
    needsGoals,
    needsAccounts,
  };
}
