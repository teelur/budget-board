import dayjs from "~/shared/dayjs";
import { ITransaction } from "~/models/transaction";
import { IBudget } from "~/models/budget";
import { IGoalResponse } from "~/models/goal";
import { IAccountResponse } from "~/models/account";
import { IAccountType } from "~/models/accountType";
import {
  ExpressionToken,
  MetricDateRangeExpression,
  MetricDataRequirements,
  MetricToken,
} from "~/models/metricWidget";
import { CategoryTypes } from "~/models/category";
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

export const METRIC_RANGE_ENDPOINTS = [
  "today",
  "this_month:16",
  "last_month:16",
  "month[-2]:16",
  "last_month:end",
  "this_month:end",
  "week[-1]:start",
  "week[0]:end",
  "year[0]:start",
  "year[0]:end",
] as const;

export const METRIC_RANGE_EXAMPLES = [
  "start=last_month:16,end=this_month:16",
  "start=month[-2]:16,end=month[0]:16",
  "start=last_month:start,end=this_month:end",
  "start=week[-1]:start,end=week[0]:end",
] as const;

export interface ResolvedMetricPeriod {
  kind: "range" | "all_time" | "invalid";
  start?: Date;
  endExclusive?: Date;
  error?: string;
}

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
  range?: MetricDateRangeExpression;
  periodError?: string;
  params: Record<string, string>;
} {
  const parts = argsStr
    .split(",")
    .map((s) => s.trim())
    .filter(Boolean);
  let period: string | undefined;
  let start: string | undefined;
  let end: string | undefined;
  const params: Record<string, string> = {};

  parts.forEach((part, index) => {
    if (index === 0 && !part.includes("=")) {
      period = part;
    } else if (part.includes("=")) {
      const eqIdx = part.indexOf("=");
      const key = part.slice(0, eqIdx).trim();
      const value = part.slice(eqIdx + 1).trim();
      if (key === "start") {
        start = value;
      } else if (key === "end") {
        end = value;
      } else {
        params[key] = value;
      }
    }
  });

  const hasStart = start !== undefined;
  const hasEnd = end !== undefined;
  const periodError =
    hasStart !== hasEnd
      ? "A custom range requires both start and end"
      : period && hasStart
        ? "A custom range cannot be combined with a named period"
        : undefined;

  return {
    period,
    range:
      start !== undefined && end !== undefined ? { start, end } : undefined,
    periodError,
    params,
  };
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
    const { period, range, periodError, params } = parseArgs(argsStr ?? "");

    tokens.push({
      type: "expression",
      source,
      metric,
      period,
      range,
      periodError,
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

function getDateOnly(value: dayjs.ConfigType): dayjs.Dayjs {
  const valueString = typeof value === "string" ? value : undefined;
  const normalizedValue = valueString?.includes("T")
    ? valueString.slice(0, 10)
    : value;
  return dayjs(normalizedValue).startOf("day");
}

function resolveCalendarEndpoint(
  expression: string,
  referenceDate: dayjs.Dayjs,
): dayjs.Dayjs | null {
  const today = referenceDate.startOf("day");
  let resolved: dayjs.Dayjs;

  if (expression === "today") {
    resolved = today;
  } else {
    const calendarMatch =
      /^(month|week|year)(?:\[(-?\d+)\])?:(start|end|\d{1,2}|sun|mon|tue|wed|thu|fri|sat)$/i.exec(
        expression,
      );
    const aliasMatch =
      /^(this_month|last_month|next_month):(start|end|\d{1,2})$/i.exec(
        expression,
      );

    if (!calendarMatch && !aliasMatch) {
      return null;
    }

    const unit = (calendarMatch?.[1] ?? "month").toLowerCase();
    const offset = Number(calendarMatch?.[2] ?? 0);
    const selector = (
      calendarMatch?.[3] ??
      aliasMatch?.[2] ??
      ""
    ).toLowerCase();
    const alias = aliasMatch?.[1]?.toLowerCase();
    const calendarOffset =
      alias === "last_month" ? -1 : alias === "next_month" ? 1 : offset;

    if (unit === "month") {
      const month = today.add(calendarOffset, "month").startOf("month");
      if (selector === "start") {
        resolved = month;
      } else if (selector === "end") {
        resolved = month.endOf("month").startOf("day");
      } else {
        const day = Number(selector);
        if (day < 1 || day > 31) {
          return null;
        }
        resolved = month.date(Math.min(day, month.daysInMonth()));
      }
    } else if (unit === "week") {
      const week = today
        .subtract(today.day(), "day")
        .add(calendarOffset, "week");
      if (selector === "start") {
        resolved = week;
      } else if (selector === "end") {
        resolved = week.add(6, "day");
      } else {
        const weekdays: Record<string, number> = {
          sun: 0,
          mon: 1,
          tue: 2,
          wed: 3,
          thu: 4,
          fri: 5,
          sat: 6,
        };
        const weekday = weekdays[selector];
        if (weekday === undefined) {
          return null;
        }
        resolved = week.add(weekday, "day");
      }
    } else {
      const year = today.add(calendarOffset, "year").startOf("year");
      if (selector === "start") {
        resolved = year;
      } else if (selector === "end") {
        resolved = year.endOf("year").startOf("day");
      } else {
        return null;
      }
    }
  }

  return resolved;
}

function resolveLegacyPeriod(
  period: string,
  referenceDate: dayjs.Dayjs,
): ResolvedMetricPeriod {
  const now = referenceDate.startOf("day");
  let start: dayjs.Dayjs;
  let endExclusive: dayjs.Dayjs;

  if (period === "all_time") {
    return { kind: "all_time" };
  }

  const rollingMatch = /^last_(\d+)_(weeks|months|years)$/.exec(period);
  if (rollingMatch) {
    const count = Number(rollingMatch[1]);
    const unit = rollingMatch[2];
    if (count < 1) {
      return { kind: "invalid", error: "The period count must be positive" };
    }

    if (unit === "weeks") {
      start = now.subtract(now.day(), "day").subtract(count - 1, "week");
      endExclusive = now.subtract(now.day(), "day").add(1, "week");
    } else if (unit === "months") {
      start = now.startOf("month").subtract(count - 1, "month");
      endExclusive = now.startOf("month").add(1, "month");
    } else {
      start = now.startOf("year").subtract(count - 1, "year");
      endExclusive = now.startOf("year").add(1, "year");
    }

    return {
      kind: "range",
      start: start.toDate(),
      endExclusive: endExclusive.toDate(),
    };
  }

  switch (period) {
    case "this_month":
      start = now.startOf("month");
      endExclusive = start.add(1, "month");
      break;
    case "last_month":
      endExclusive = now.startOf("month");
      start = endExclusive.subtract(1, "month");
      break;
    case "this_year":
      start = now.startOf("year");
      endExclusive = start.add(1, "year");
      break;
    case "last_3_months":
    case "last_6_months":
    case "last_12_months": {
      const count = Number(period.match(/\d+/)?.[0] ?? 0);
      endExclusive = now.startOf("month").add(1, "month");
      start = endExclusive.subtract(count, "month");
      break;
    }
    default:
      return { kind: "invalid", error: `Unknown period: ${period}` };
  }

  return {
    kind: "range",
    start: start.toDate(),
    endExclusive: endExclusive.toDate(),
  };
}

export function resolveMetricPeriod(
  period: string | undefined,
  range: MetricDateRangeExpression | undefined,
  referenceDate: Date = new Date(),
  periodError?: string,
): ResolvedMetricPeriod {
  if (periodError) {
    return { kind: "invalid", error: periodError };
  }

  const reference = getDateOnly(referenceDate);
  if (!range) {
    return resolveLegacyPeriod(period ?? "this_month", reference);
  }

  const start = resolveCalendarEndpoint(range.start, reference);
  const end = resolveCalendarEndpoint(range.end, reference);
  if (!start || !end || !start.isValid() || !end.isValid()) {
    return { kind: "invalid", error: "Invalid custom range endpoint" };
  }
  if (start.isAfter(end, "day")) {
    return { kind: "invalid", error: "Custom range start is after its end" };
  }

  return {
    kind: "range",
    start: start.toDate(),
    endExclusive: end.add(1, "day").toDate(),
  };
}

function getMonthsForResolvedPeriod(period: ResolvedMetricPeriod): Date[] {
  if (period.kind !== "range" || !period.start || !period.endExclusive) {
    return [];
  }

  const lastIncludedMonth = dayjs(period.endExclusive)
    .subtract(1, "day")
    .startOf("month");
  const months: Date[] = [];
  let month = dayjs(period.start).startOf("month");
  while (
    month.isBefore(lastIncludedMonth, "month") ||
    month.isSame(lastIncludedMonth, "month")
  ) {
    months.push(month.toDate());
    month = month.add(1, "month");
  }
  return months;
}

export function getMonthsForPeriod(
  period: string,
  range?: MetricDateRangeExpression,
  periodError?: string,
  referenceDate: Date = new Date(),
): Date[] {
  return getMonthsForResolvedPeriod(
    resolveMetricPeriod(period, range, referenceDate, periodError),
  );
}

export function isInPeriod(
  isoDateStr: string,
  period: string | undefined,
  range?: MetricDateRangeExpression,
  periodError?: string,
  referenceDate: Date = new Date(),
): boolean {
  // Compare using date-only values to avoid timezone-driven month boundary shifts.
  const d = getDateOnly(isoDateStr);
  if (!d.isValid()) {
    return false;
  }

  const resolved = resolveMetricPeriod(
    period,
    range,
    referenceDate,
    periodError,
  );
  return isInResolvedPeriod(d, resolved);
}

function isInResolvedPeriod(
  date: dayjs.Dayjs,
  resolved: ResolvedMetricPeriod,
): boolean {
  if (resolved.kind === "all_time") {
    return true;
  }
  if (resolved.kind !== "range" || !resolved.start || !resolved.endExclusive) {
    return false;
  }

  const start = dayjs(resolved.start);
  const endExclusive = dayjs(resolved.endExclusive);
  return !date.isBefore(start, "day") && date.isBefore(endExclusive, "day");
}

export interface MetricDataContext {
  transactions: ITransaction[];
  budgets: IBudget[];
  goals: IGoalResponse[];
  accounts: IAccountResponse[];
  accountTypes: IAccountType[];
  getCategoryType: (category: string) => string;
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

export function hasCurrencyMetric(tokens: MetricToken[]): boolean {
  return getAllExpressionTokens(tokens).some(
    (token) => getMetricFormat(token) === "currency",
  );
}

function resolveTransactions(
  metric: string,
  period: ResolvedMetricPeriod,
  params: Record<string, string>,
  ctx: MetricDataContext,
): number {
  const type = params.type ?? "all";
  const category = params.category;

  let txs = getVisibleTransactions(ctx.transactions).filter((t) =>
    isInResolvedPeriod(getDateOnly(t.date), period),
  );

  if (type === "expense") {
    txs = txs.filter(
      (t) => ctx.getCategoryType(t.category ?? "") !== CategoryTypes.Income,
    );
  } else if (type === "income") {
    txs = txs.filter(
      (t) => ctx.getCategoryType(t.category ?? "") === CategoryTypes.Income,
    );
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
      if (txs.length === 0) {
        return 0;
      }
      const avg = txs.reduce((n, t) => n + t.amount, 0) / txs.length;
      return type === "expense" ? Math.abs(avg) : avg;
    }
    default:
      return 0;
  }
}

function resolveBudgets(
  metric: string,
  period: ResolvedMetricPeriod,
  params: Record<string, string>,
  ctx: MetricDataContext,
): number {
  const category = params.category;
  if (!category) {
    return 0;
  }

  const budgets = ctx.budgets
    .filter((b) =>
      isInResolvedPeriod(
        getDateOnly(dayjs(b.month).format("YYYY-MM-DD")),
        period,
      ),
    )
    .filter((b) => areStringsEqual(b.category, category));

  const total = budgets.reduce((n, b) => n + b.limit, 0);
  if (metric === "total") {
    return total;
  }

  const txs = getVisibleTransactions(ctx.transactions)
    .filter((t) => isInResolvedPeriod(getDateOnly(t.date), period))
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
  const name = params.name;
  if (!name) {
    return 0;
  }

  const goal = ctx.goals.find((g) => areStringsEqual(g.name, name));
  if (!goal) {
    return 0;
  }

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
  if (metric !== "balance") {
    return 0;
  }

  const visible = filterVisibleAccounts(ctx.accounts);
  const typeName = params.type;
  const accountName = params.name;

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
    const period = resolveMetricPeriod(
      token.period,
      token.range,
      new Date(),
      token.periodError,
    );
    if (period.kind === "invalid") {
      return "[invalid period]";
    }
    if (token.range && token.source !== "transactions") {
      return "[unsupported period]";
    }

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
      if (token.type === "literal") {
        return token.text;
      }
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

  const transactionMonthsMap = new Map<string, Date>();
  const budgetMonthsMap = new Map<string, Date>();
  let needsAllTimeTransactions = false;

  expressions.forEach((expression) => {
    const period = resolveMetricPeriod(
      expression.period,
      expression.range,
      new Date(),
      expression.periodError,
    );

    if (
      (expression.source === "transactions" ||
        expression.source === "budgets") &&
      period.kind === "all_time"
    ) {
      needsAllTimeTransactions = true;
    }

    if (
      (expression.source === "transactions" ||
        expression.source === "budgets") &&
      period.kind === "range" &&
      !(expression.source === "budgets" && expression.range)
    ) {
      getMonthsForResolvedPeriod(period).forEach((date) => {
        transactionMonthsMap.set(
          `${date.getFullYear()}-${date.getMonth()}`,
          date,
        );
      });
    }

    if (
      expression.source === "budgets" &&
      !expression.range &&
      period.kind === "range"
    ) {
      getMonthsForResolvedPeriod(period).forEach((date) => {
        budgetMonthsMap.set(`${date.getFullYear()}-${date.getMonth()}`, date);
      });
    }
  });

  if (needsAllTimeTransactions) {
    transactionMonthsMap.clear();
  }

  if (needsAllTimeTransactions) {
    return {
      needsTransactions,
      transactionMonths: [],
      needsAllTimeTransactions,
      needsBudgets,
      budgetMonths: Array.from(budgetMonthsMap.values()),
      needsGoals,
      needsAccounts,
    };
  }

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
