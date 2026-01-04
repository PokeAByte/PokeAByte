import { Advanced } from "@/components/Advanced";
import { GithubSettingsPanel } from "./panels/GithubSettingsPanel";
import { AppSettingsPanel } from "./panels/AppSettingsPanel";
import { UISettingsPanel } from "./panels/UISettingsPanel";

export function Settings() {
	return (
		<article class="margin-top">
			<Advanced>
				<GithubSettingsPanel />
			</Advanced>
			<AppSettingsPanel />
			<UISettingsPanel />
		</article>
	);
}
