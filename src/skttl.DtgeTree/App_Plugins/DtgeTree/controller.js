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

						newDtge.Icon = type.icon;
						newDtge.Name = type.name;
						newDtge.Alias = type.alias;

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
				return p.alias == alias
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

			vm.dtge.AllowedDocTypes = getPropertyValue(vm.properties, "allowedDocTypes", "").split(",");
			vm.dtge.ViewPath = getPropertyValue(vm.properties, "viewPath", "");
			vm.dtge.EnablePreview = getPropertyValue(vm.properties, "enablePreviews", vm.dtge.EnablePreviews ? "1" : "0") == "1";
			vm.dtge.PreviewViewPath = getPropertyValue(vm.properties, "previewViewPath", "");

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

			vm.properties = [

				{
					label: "Allowed doctypes",
					description: "Specify the doctypes allowed in this editor.",
					view: "textbox",
					alias: "allowedDocTypes",
					value: dtge.AllowedDocTypes.join(",")
				},
				{
					label: "View path",
					description: "Specify the path to the view. If not specified, the default (/Views/Partials/TypedGrid/Editors/) will be used.",
					view: "textbox",
					alias: "viewPath",
					value: dtge.ViewPath
				},
				{
					label: "Enable previews",
					description: "Enables previews of the grid editor in the backoffice",
					view: "boolean",
					alias: "enablePreviews",
					value: dtge.EnablePreview
				},
				{
					label: "Preview view path",
					description: "Specify the path to the preview. If not specified, the default (/Views/Partials/TypedGrid/Editors/Previews/) will be used.",
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