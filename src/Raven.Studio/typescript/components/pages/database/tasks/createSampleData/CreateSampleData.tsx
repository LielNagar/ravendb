﻿import React from "react";
import { Icon } from "components/common/Icon";
import { Button, Card, CardBody, Collapse, Spinner } from "reactstrap";
import "./CreateSampleData.scss";
import useBoolean from "components/hooks/useBoolean";
import { useAsync, useAsyncCallback } from "react-async-hook";
import { useServices } from "components/hooks/useServices";
import copyToClipboard from "common/copyToClipboard";
import appUrl from "common/appUrl";
import Code from "components/common/Code";
import { LoadError } from "components/common/LoadError";
import SmokeSvg from "./CreateSampleDataSmoke";
import ButtonWithSpinner from "components/common/ButtonWithSpinner";
import { useRavenLink } from "components/hooks/useRavenLink";
import collectionsTracker from "common/helpers/database/collectionsTracker";
import { NonShardedViewProps } from "components/models/common";

function CreateSampleData({ db }: NonShardedViewProps) {
    const { tasksService } = useServices();

    const { value: isCodeSampleOpen, toggle: toggleCodeSample } = useBoolean(false);

    const docsLink = useRavenLink({ hash: "SUTS29" });

    const asyncFetchCollectionsStats = useAsync(() => tasksService.fetchCollectionsStats(db.name), []);
    const asyncGetSampleDataClasses = useAsync(() => tasksService.getSampleDataClasses(db.name), []);
    const asyncCreateSampleData = useAsyncCallback(async () => {
        await tasksService.createSampleData(db.name);

        if (db.hasRefreshConfiguration()) {
            return;
        }

        const dbInfo = await tasksService.getDatabaseForStudio(db.name);
        if (!dbInfo.HasRevisionsConfiguration) {
            return;
        }

        db.hasRevisionsConfiguration(true);
        collectionsTracker.default.configureRevisions(db);
    });

    const canCreateSampleData = asyncFetchCollectionsStats.result
        ? asyncFetchCollectionsStats.result.collections.filter((x) => x.documentCount() > 0).length === 0
        : false;

    return (
        <div className="sample-data absolute-fill">
            <SmokeSvg />
            <div className="scroll">
                <div className="center">
                    <div className="flex-vertical flex-center text-center">
                        <p className="lead text-emphasis margin-bottom">
                            This action populates the database with various collections, .json documents, indexes, and
                            document extensions such as time series, counters, attachments, and revisions.
                        </p>
                        <p className="small">
                            The{" "}
                            <a href={docsLink} target="_blank">
                                RavenDB documentation
                            </a>{" "}
                            includes numerous examples that are based on this sample data. This is a simple and
                            effective way to familiarize yourself with RavenDB features and experiment with the data.
                        </p>
                        <div className="margin-top margin-bottom-sm">
                            <ButtonWithSpinner
                                size="lg"
                                className="rounded-pill"
                                color="primary"
                                onClick={asyncCreateSampleData.execute}
                                isSpinning={asyncCreateSampleData.status === "loading"}
                                icon={asyncCreateSampleData.status === "success" ? "check" : "magic-wand"}
                                disabled={
                                    !canCreateSampleData ||
                                    asyncCreateSampleData.status === "loading" ||
                                    asyncCreateSampleData.status === "success"
                                }
                            >
                                {asyncCreateSampleData.status === "success"
                                    ? "Sample data created"
                                    : "Create sample data"}
                            </ButtonWithSpinner>

                            {asyncFetchCollectionsStats.status === "success" && !canCreateSampleData && (
                                <div className="padding padding-xs margin-top-sm text-warning">
                                    <Icon icon="warning" />
                                    Requires an empty database
                                </div>
                            )}

                            {asyncCreateSampleData.status === "success" && (
                                <div className="padding padding-xs margin-top-sm">
                                    <a href={appUrl.forDocuments("", db)}>
                                        <Icon icon="arrow-thin-right" />
                                        Go to documents
                                    </a>
                                </div>
                            )}
                        </div>
                        <Button size="sm" className="rounded-pill margin-bottom-md" onClick={toggleCodeSample}>
                            <Icon icon="hash" /> {isCodeSampleOpen ? "Hide" : "Show"} C# classes
                        </Button>
                    </div>
                    <Collapse isOpen={isCodeSampleOpen}>
                        <Card className="sample-code">
                            <CardBody>
                                <div className="sample-code-header">
                                    <h3>Sample data C# code</h3>
                                    {asyncGetSampleDataClasses.result && (
                                        <Button
                                            className="rounded-pill"
                                            onClick={() =>
                                                copyToClipboard.copy(
                                                    asyncGetSampleDataClasses.result,
                                                    "Copied C# classes to clipboard."
                                                )
                                            }
                                        >
                                            <Icon icon="copy" /> <span>Copy C# classes</span>
                                        </Button>
                                    )}
                                </div>
                                {asyncGetSampleDataClasses.loading && (
                                    <div className="d-flex justify-content-center">
                                        <Spinner className="spinner-gradient" />
                                    </div>
                                )}
                                {asyncGetSampleDataClasses.error && (
                                    <LoadError
                                        error="Unable to load sample data classes"
                                        refresh={asyncGetSampleDataClasses.execute}
                                    />
                                )}
                                {asyncGetSampleDataClasses.result && (
                                    <Code code={asyncGetSampleDataClasses.result} language="csharp" />
                                )}
                            </CardBody>
                        </Card>
                    </Collapse>
                </div>
            </div>
        </div>
    );
}

export default CreateSampleData;
