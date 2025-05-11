import React, { FormEvent } from "react";
import { Store } from "../../utility/propertyStore";
import { useAPI } from "../../hooks/useAPI";

export function GithubSettings() {
	const filesClient = Store.client.files;
	const githubSettings = useAPI(Store.client.files.getGithubSettings);
	const [status, setStatus] = React.useState({ color: "", message: "" });
	React.useEffect(
		() => githubSettings.call(),
		// eslint-disable-next-line react-hooks/exhaustive-deps
		[]
	);
	if (!githubSettings.wasCalled || githubSettings.isLoading) {
		return <></>;
	}

	const openGithubLink = () => {
		filesClient.getGithubLink().then(uri => {
			if (uri !== null) {
				window.open(uri, "_blank")
			}
		});
	}
	const testGithubSettings = () => {
		filesClient.testGithubSettingsAsync().then(response => {
			if (response?.startsWith("Successfully")) {
				setStatus({ color: "success-text", message: response });
			} else if (response?.startsWith("Failed")) {
				setStatus({ color: "error-text", message: response });
			}
		});
	}
	const onSubmit = (event: FormEvent<HTMLFormElement>) => {
		const formData = new FormData(event.currentTarget);
		filesClient.saveGithubSettings({
			owner: formData.get("owner") as string ?? "",
			repo: formData.get("repo") as string ?? "",
			dir: formData.get("dir") as string ?? "",
			token: formData.get("token") as string ?? "",
			accept: githubSettings.result?.accept || "",
			api_version: githubSettings.result?.api_version || "",
		}).then(
			(ok) => {
				if (ok) {
					setStatus({ color: "success-text", message: "Changes saved successfully!" });
				} else {
					setStatus({ color: "error-text", message: "Failed to save github settings!!" });
				}
			}
		)
		event.preventDefault();
		return false;
	}
	return (
		<article>
			<h3 className="small"><strong>GitHub Api Settings</strong></h3>
			<div>
				<hr />
				<p className="error-text large-line">
					Changing these settings is generally not recommended unless you know what you are doing.
					These settings allow you to set a custom mapper repository, incorrect settings can cause issues retrieving
					new and updated mappers. Please only do so with caution.
				</p>
				<hr />
				<br />
				<form onSubmit={onSubmit}>
					<div className="field">
						<label htmlFor="owner">GitHub Account Name: </label>
						<input name="owner" type="text" defaultValue={githubSettings.result?.owner}></input>
					</div>
					<div className="field">
						<label htmlFor="repo">Repository Name:</label>
						<input name="repo" type="text" defaultValue={githubSettings.result?.repo}></input>
					</div>
					<div className="field">
						<label htmlFor="dir">
							Alternative Directory Name:
						</label>
						<input name="dir" type="text" defaultValue={githubSettings.result?.dir}></input>
						<br />
					</div>
					<small>
						Note: This is only required if the mapper_tree.json is not in the root directory of the repository.
					</small>
					<div className="field">
						<label htmlFor="token">
							Personal Access Token:
						</label>
						<input name="token" type="text" defaultValue={githubSettings.result?.token}></input>
					</div>
					<small>
						Note: This is required if the repository is private or if you want to bypass the
						GitHub REST Api rate limit. Once you have a token generated do not share it with anyone; for more
						information please refer to this page:{" "}
						<a href="https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens">
							Managing your personal access tokens
						</a>
					</small>
					<br />
					{status.message !== "" &&
						<span className={status.color}>{status.message}</span>
					}
					<br />
					<div className="row wrap">
						<button className="border-blue margin-right" onClick={testGithubSettings} type="button">
							TEST SETTINGS
						</button>
						<button className="border-purple margin-right" onClick={openGithubLink} type="button">
							OPEN GITHUB LINK
						</button>
						<button className="border-green margin-right" type="submit">
							SAVE SETTINGS
						</button>
						<button className="border-red">CLEAR SETTINGS</button>
					</div>
				</form>
			</div>
		</article>
	)
}