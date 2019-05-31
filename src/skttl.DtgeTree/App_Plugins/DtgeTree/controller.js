(function () {
    "use strict";

    function DgteTreeEditController($scope, $http, $routeParams, $injector, contentTypeResource, dataTypeResource, editorState, contentEditingHelper, formHelper, navigationService, iconHelper, contentTypeHelper, notificationsService, $filter, $q, localizationService, overlayHelper, eventsService) {

        var vm = this;
        var localizeSaving = localizationService.localize("general_saving");
        var evts = [];

        vm.save = save;

        vm.currentNode = null;
        vm.dtge = {};

        vm.page = {};
        vm.page.loading = false;
        vm.page.saveButtonState = "init";

        vm.properties = [];

        if ($routeParams.create) {
            vm.page.loading = true;

            //we are creating so get an empty data type item
            $http.get("/umbraco/backoffice/DtgeTree/Manifest/ScaffoldManifest/").then(function (dt) {

                if ($routeParams.create !== true) {
                    $http.get("/umbraco/backoffice/UmbracoApi/ContentType/GetById?id=" + $routeParams.create).then(function (type) {
                        var newDtge = dt.data;

                        newDtge.Icon = type.data.icon;
                        newDtge.Name = type.data.name;
                        newDtge.Alias = type.data.alias;
                        newDtge.AllowedDocTypes.push(type.data.alias);

                        init(newDtge);

                    });
                }
                else {

                    init(dt.data);
                }

                vm.page.loading = false;
            });
        }
        else {
            loadDtge();
        }

        function loadDtge() {
            vm.page.loading = true;
            $http.get("/umbraco/backoffice/DtgeTree/Manifest/GetManifest/?alias=" + $routeParams.id).then(function (dt) {
                init(dt.data);
                syncTreeNode(vm.dtge, dt.data.Alias, true);
                vm.page.loading = false;
            });
        }



        function getPropertyValue(properties, alias, defaultValue) {
            var property = properties.filter(function (p) {
                return p.alias == alias;
            });

            if (property.length > 0) {
                return property[0].value;
            }
            else {
                return defaultValue;
            }
        }

        /* ---------- SAVE ---------- */

        function save() {

            var deferred = $q.defer();

            vm.page.saveButtonState = "busy";

            vm.dtge.AllowedDocTypes = getPropertyValue(vm.properties, "allowedDocTypes", []).map(function (i) {
                return i.value;
            });
            vm.dtge.ViewPath = getPropertyValue(vm.properties, "viewPath", "");
            vm.dtge.EnablePreview = getPropertyValue(vm.properties, "enablePreviews", vm.dtge.EnablePreviews ? "1" : "0") == "1";
            vm.dtge.PreviewViewPath = getPropertyValue(vm.properties, "previewViewPath", "");
            vm.dtge.LargeDialog = getPropertyValue(vm.properties, "largeDialog", vm.dtge.LargeDialog ? "1" : "0") == "1";
            vm.dtge.ShowDocTypeSelectAsGrid = getPropertyValue(vm.properties, vm.dtge.ShowDocTypeSelectAsGrid ? "1" : "0") == "1";

            $http.post("/umbraco/backoffice/DtgeTree/Manifest/SaveManifest", vm.dtge).then(function (data) {

                syncTreeNode(vm.dtge, vm.dtge.Alias);
                vm.page.saveButtonState = "success";

                deferred.resolve(data);
            }, function (err) {
                //error
                if (err) {
                    editorState.set($scope.content);
                }
                else {
                    localizationService.localize("speechBubbles_validationFailedHeader").then(function (headerValue) {
                        localizationService.localize("speechBubbles_validationFailedMessage").then(function (msgValue) {
                            notificationsService.error(headerValue, msgValue);
                        });
                    });
                }
                vm.page.saveButtonState = "error";
                deferred.reject(err);
            });

        }

        function init(dtge) {

            //set a shared state
            editorState.set(dtge);
            vm.dtge = dtge;
            console.log(dtge);
            vm.properties = [

                {
                    label: "Allowed doctypes",
                    description: "Strings can be REGEX patterns to allow matching groups of doc types in a single entry. e.g. \"Widget$\" will match all doc types with an alias ending in \"Widget\". However if a single doc type is matched, (aka **Single Doc Type Mode**), then dropdown selection stage (in the DTGE panel) will be skipped.",
                    view: "multipletextbox",
                    alias: "allowedDocTypes",
                    config: {
                        "min": 0,
                        "max": 0
                    },
                    value: dtge.AllowedDocTypes.map(function (value) { return { value }; })
                },
                {
                    label: "Enable previews",
                    description: "Enables rendering a preview of the grid cell in the grid editor.",
                    view: "boolean",
                    alias: "enablePreviews",
                    value: dtge.EnablePreview ? "1" : "0"
                },
                {
                    label: "Use large dialog",
                    description: "Makes the editing dialog larger. Especially useful for grid editors with complex property editors.",
                    view: "boolean",
                    alias: "largeDialog",
                    value: dtge.LargeDialog ? "1" : "0"
                },
                {
                    label: "Select content type using",
                    description: "Makes the content type selection dialog render a grid, in stead of the default list with descriptions",
                    view: "boolean",
                    alias: "showDocTypeSelectAsGrid",
                    value: dtge.ShowDocTypeSelectAsGrid ? "1" : "0"
                },
                {
                    label: "View path",
                    description: "Set's an alternative view path for where the **Doc Type Grid Editor** should look for views when rendering. Defaults to `~/Views/Partials/TypedGrid/Editors/`",
                    view: "textbox",
                    alias: "viewPath",
                    value: dtge.ViewPath
                },
                {
                    label: "Preview view path",
                    description: "Set's an alternative view path for where the **Doc Type Grid Editor** should look for views when rendering. Defaults to `~/Views/Partials/TypedGrid/Editors/Previews/",
                    view: "textbox",
                    alias: "previewViewPath",
                    value: dtge.PreviewViewPath
                }
            ]
        }

        /** Syncs the content type  to it's tree node - this occurs on first load and after saving */
        function syncTreeNode(dt, path, initialLoad) {
            navigationService.syncTree({ tree: "dtges", path: path.split(","), forceReload: initialLoad !== true }).then(function (syncArgs) {
                vm.currentNode = syncArgs.node;
            });
        }

        evts.push(eventsService.on("app.refreshEditor", function (name, error) {
            loadDtge();
        }));

        //ensure to unregister from all events!
        $scope.$on('$destroy', function () {
            for (var e in evts) {
                eventsService.unsubscribe(evts[e]);
            }
        });
    }

    angular.module("umbraco").controller("DtgeTree.EditController", DgteTreeEditController);
})();