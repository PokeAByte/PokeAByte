import { Panel } from "../../../components/Panel";
import { saveSetting, UISettings, uiSettingsSignal } from "@/Contexts/uiSettingsSignal";
import { FavoriteManagement } from "./components/FavoriteManagement";
import { useComputed } from "@preact/signals";

export function UISettingsPanel() {
	return (
		<Panel id="settings_ui" title="UI settings">
			<strong>
				Settings will apply as soon as you change them.
			</strong>
			<hr />
			<form onSubmit={(e) => e.preventDefault()}>
				<table class="striped">
					<tbody>
						<SettingSwitch 
							setting="advancedMode" 
							label="Enable advanced mode:"
							description="Displays additional information and enables certain features."
						/>
						<SettingSwitch 
							setting="forceVisible" 
							label="Display hidden properties:"
							description="Enabling this shows properties even if you chose to hide them."
						/>
						<SettingSwitch 
							setting="preserveFreeze" 
							label="Preserve freezes on reload:"
							description="When reloading a mapper, reapply previously frozen values."
						/>
						<SettingSwitch 
							setting="recentlyUsedEnabled" 
							label="Track recently used mappers:"
							description="When enabled, keep track of the last 5 used mappers for quick loading."
						/>
						<FavoriteManagement />
						<SettingSwitch 
							setting="stickyHeader" 
							label="Sticky header:"
							description="Keep header visible when scrolled down."
						/>
					</tbody>
				</table>
			</form>
		</Panel>
	)
}

type BooleanUiSettings = { 
	[K in keyof UISettings as UISettings[K] extends boolean|undefined ? K : never]: UISettings[K] 
};

function SettingSwitch(props: {setting: keyof BooleanUiSettings, label: string, description?: string}) {
	const value = useComputed(() => uiSettingsSignal.value[props.setting]).value;
	return (
		<tr>
			<th>
				<label htmlFor={props.setting}>{props.label}</label>
			</th>
			<td>
				<input
					name={props.setting}
					type="checkbox"
					role="switch"
					checked={value}
					onInput={() => saveSetting(props.setting, !value)}
				/>
				{props.description 
					? <span>{props.description}</span> 
					: null
				}				
			</td>
		</tr>
	);
}