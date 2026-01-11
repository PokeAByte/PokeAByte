import { GithubSettingsPanel } from "./panels/GithubSettingsPanel";
import { AppSettingsPanel } from "./panels/AppSettingsPanel";
import { UISettingsPanel } from "./panels/UISettingsPanel";
import { Show } from "@preact/signals/utils";
import { advancedModeSignal } from "@/Contexts/uiSettingsSignal";

export function Settings() {
	return (
		<article class="margin-top">
			<Show when={advancedModeSignal}>
				<GithubSettingsPanel />
			</Show>
			<AppSettingsPanel />
			<UISettingsPanel />
		</article>
	);
}
