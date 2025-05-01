import { Toasts } from "../../../notifications/ToastStore";

export function clipboardCopy(value: string | null | undefined) {
  if (value) {
    Toasts.push(`Copied ${value} to the clipboard`, "info", "blue");
    navigator.clipboard.writeText(value);
  }
}