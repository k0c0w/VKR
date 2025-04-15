import L from "leaflet";
import "./style.css";

L.Control.LevelPicker = L.Control.extend({
    options: {
        position: "topright",
        levelLabels: [],
        selectedLevelIndex: 0,
        disableRubbishBin: false,
        showLevelButtons: true,
        controlDisabled: false,
    },

    initialize: function (options) {
        L.Util.setOptions(this, options);
        this.levelLabels = this.options.levelLabels || ["1"];
        this.selectedLevelIndex = this.options.selectedLevelIndex || 0;
        this.disableRubbishBin = this.options.disableRubbishBin;
        this.showLevelButtons = this.options.showLevelButtons;
        this.controlDisabled = this.options.controlDisabled;
    },

    onAdd: function (map) {
        this._map = map;

        if (!map.hasOwnProperty("levelControl")) {
            map.levelControl = this;
        }

        const mainContainer = L.DomUtil.create("div", "level-picker-container");
        L.DomEvent.disableClickPropagation(mainContainer);
        L.DomEvent.on(mainContainer, "mousewheel", L.DomEvent.stopPropagation);
        if (this.controlDisabled) {
            L.DomUtil.addClass(mainContainer, "leaflet-control-disabled");
        }

        const selectContainer = L.DomUtil.create("div", "level-picker leaflet-bar", mainContainer);

        this.levelSelector = L.DomUtil.create("select", "level-picker-select", selectContainer);
        this._populateSelectOptions();
        this.levelSelector.value = this.selectedLevelIndex.toString();
        this.levelSelector.disabled = this.controlDisabled;
        L.DomEvent.on(this.levelSelector, "change", this._onSelectChange, this);

        if (this.showLevelButtons) {
            this._createButtonContainer(mainContainer);
        }

        this._container = mainContainer;
        return mainContainer;
    },

    onRemove: function () {
        L.DomEvent.off(this.levelSelector, "change", this._onSelectChange, this);
        if (this.showLevelButtons) {
            this._removeButtonEventListeners();
        }
    },

    _createButtonContainer: function (parentContainer) {
        this.buttonContainer = L.DomUtil.create("div", "level-picker-buttons", parentContainer);

        this.rubbishBinButton = L.DomUtil.create("button", "level-picker-button level-picker-rubbish-bin", this.buttonContainer);
        this.rubbishBinButton.innerHTML = '';
        this.rubbishBinButton.disabled = this.controlDisabled || this.disableRubbishBin;
        L.DomEvent.on(this.rubbishBinButton, "click", this._onRubbishBinClick, this);

        this.editButton = L.DomUtil.create("button", "level-picker-button level-picker-edit", this.buttonContainer);
        this.editButton.innerHTML = '';
        this.editButton.disabled = this.controlDisabled;
        L.DomEvent.on(this.editButton, "click", this._onEditClick, this);

        this.plusButton = L.DomUtil.create("button", "level-picker-button level-picker-plus", this.buttonContainer);
        this.plusButton.innerHTML = '';
        this.plusButton.disabled = this.controlDisabled;
        L.DomEvent.on(this.plusButton, "click", this._onPlusClick, this);
    },

    _removeButtonEventListeners: function () {
        if (this.rubbishBinButton) {
            L.DomEvent.off(this.rubbishBinButton, "click", this._onRubbishBinClick, this);
        }
        if (this.editButton) {
            L.DomEvent.off(this.editButton, "click", this._onEditClick, this);
        }
        if (this.plusButton) {
            L.DomEvent.off(this.plusButton, "click", this._onPlusClick, this);
        }
    },

    _populateSelectOptions: function () {
        this.levelSelector.innerHTML = "";
        this.levelLabels.forEach((label, index) => {
            const option = L.DomUtil.create("option", "", this.levelSelector);
            option.value = index.toString();
            option.innerHTML = label;
        });
    },

    _onSelectChange: function () {
        const newLevel = parseInt(this.levelSelector.value, 10);
        this.changeLevel(newLevel);
    },

    _onRubbishBinClick: function () {
        if (!this.rubbishBinButton.disabled) {
            this._map.fire("levelpicker:removelevelclicked", {
                levelIndex: this.selectedLevelIndex,
                label: this.levelLabels[this.selectedLevelIndex],
            });
        }
    },

    _onEditClick: function () {
        if (!this.editButton.disabled) {
            this._map.fire("levelpicker:editlevelbuttonclicked", {
                levelIndex: this.selectedLevelIndex,
                label: this.levelLabels[this.selectedLevelIndex],
            });
        }
    },

    _onPlusClick: function () {
        if (!this.plusButton.disabled) {
            this._map.fire("levelpicker:addbuttonclicked");
        }
    },

    changeLevel: function (levelIndex) {
        if (
            levelIndex < 0 ||
            levelIndex >= this.levelLabels.length ||
            levelIndex === this.selectedLevelIndex
        ) {
            return;
        }

        this.selectedLevelIndex = levelIndex;
        this._map.level = levelIndex;
        this.levelSelector.value = levelIndex.toString();

        this._map.fire("levelpicker:changelevel", { levelIndex: levelIndex, label: this.levelLabels[levelIndex] });
    },

    setLevels: function (levelLabels, selectLevelIndex) {
        const oldLevel = this._map.level;

        this.levelLabels = levelLabels || this.levelLabels;
        this.selectedLevelIndex = selectLevelIndex !== undefined ? selectLevelIndex : this.selectedLevelIndex;

        if (this.selectedLevelIndex < 0 || this.selectedLevelIndex >= this.levelLabels.length) {
            this.selectedLevelIndex = 0;
        }

        this._populateSelectOptions();
        this.levelSelector.value = this.selectedLevelIndex.toString();
        this._map.level = this.selectedLevelIndex;

        this._map.fire("levelpicker:setLevels", {
            labels: this.levelLabels,
        });
        
        if (oldLevel !== this.selectedLevelIndex) {
            this._map.fire("levelpicker:changelevel", {
                levelIndex: this.selectedLevelIndex,
                label: this.levelLabels[this.selectedLevelIndex],
            });
        }
    },

    setRubbishBinDisabled: function (disabled) {
        if (this.rubbishBinButton) {
            this.rubbishBinButton.disabled = disabled || this.controlDisabled;
        }
    },

    setControlDisabled: function (disabled) {
        this.controlDisabled = disabled;
        if (this.levelSelector) {
            this.levelSelector.disabled = disabled;
        }
        if (this.rubbishBinButton) {
            this.rubbishBinButton.disabled = disabled || this.disableRubbishBin;
        }
        if (this.editButton) {
            this.editButton.disabled = disabled;
        }
        if (this.plusButton) {
            this.plusButton.disabled = disabled;
        }
        const mainContainer = this._container;
        if (mainContainer) {
            if (disabled) {
                L.DomUtil.addClass(mainContainer, "leaflet-control-disabled");
            } else {
                L.DomUtil.removeClass(mainContainer, "leaflet-control-disabled");
            }
        }
    },

    setShowLevelButtons: function (show) {
        if (this.showLevelButtons === show) {
            return;
        }

        this.showLevelButtons = show;
        const mainContainer = this._container;

        if (!show && this.buttonContainer) {
            this._removeButtonEventListeners();
            mainContainer.removeChild(this.buttonContainer);
            this.buttonContainer = null;
            this.rubbishBinButton = null;
            this.editButton = null;
            this.plusButton = null;
        } else if (show && !this.buttonContainer) {
            this._createButtonContainer(mainContainer);
        }
    },
});
