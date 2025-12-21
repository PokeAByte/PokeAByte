import { Advanced } from "@/components/Advanced";
import { GithubSettingsPanel } from "./panels/GithubSettingsPanel";
import { AppSettingsPanel } from "./panels/AppSettingsPanel";

export function Settings() {
	return (
		<article class="margin-top">
			<Advanced>
				<GithubSettingsPanel />
			</Advanced>
			<AppSettingsPanel />
		</article>
	);
}
