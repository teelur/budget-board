import dayjs from "dayjs";
import customParseFormat from "dayjs/plugin/customParseFormat";
import isSameOrAfter from "dayjs/plugin/isSameOrAfter";
import isSameOrBefore from "dayjs/plugin/isSameOrBefore";
import isBetween from "dayjs/plugin/isBetween";
import localizedFormat from "dayjs/plugin/localizedFormat";

// Import locale data
import "dayjs/locale/de";
import "dayjs/locale/en";
import "dayjs/locale/fr";
import "dayjs/locale/zh-cn";

// Extend dayjs with the necessary plugins

dayjs.extend(customParseFormat);
dayjs.extend(isSameOrAfter);
dayjs.extend(isSameOrBefore);
dayjs.extend(isBetween);
dayjs.extend(localizedFormat);

export default dayjs;
