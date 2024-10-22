// GLOBAL VARIABLES
const qtyInputs = [];

// CLASS FOR DYNAMIC TAB GENERATION
class DynamicTabs {
    static tabCounter = 0;
    static instace = null;
    constructor(tabLinksContainer, tabContentsContainer, contentType, rowData, ...dataTables) {
        this.tabLinksContainer = document.getElementById(tabLinksContainer);
        this.tabContentsContainer = document.getElementById(tabContentsContainer);
        this.contentType = contentType || '';
        this.rowData = rowData;
        this.dataTables = dataTables
        this.id = ++DynamicTabs.tabCounter;
        this.initialize();
        this.bindMethods();
        
        DynamicTabs.instance = this;
    }

    initialize() {
        this.addTab(this.contentType);
    }

    // for inline scripts to run globally
    bindMethods() {
        window.removeLot = this.removeLot.bind(this);
        window.updateSetQty = this.updateSetQty.bind(this);
    }

    addTab(content) {
        
        const id = `${this.id}`;
        const tabId = `${this.id}-tab`;
        const contentId = `${this.id}-content`;
        const contentType = content || this.contentType 
        const rowData = this.rowData;
        const tabContentHTML = this.getHtmlContent(contentType, id, rowData, tabId, contentId)

        const tabLink = this.createTabLink(tabId, contentId, this.rowData.Model);
        const tabContent = this.createTabContent(contentId, tabContentHTML);
        

        this.tabLinksContainer.appendChild(tabLink);
        this.tabContentsContainer.appendChild(tabContent);

        this.setupTabContent(contentType, id);
        this.showTab(tabLink);
        if (content !== 'checkpoint' || this.rowData.Manual) this.addCloseTabListener(tabLink, tabId, contentId);
    }

    // dynamic tab methods link and content creation
    createTabLink(tabId, contentId, model) {
        const tabLink = document.createElement('li');
        tabLink.classList.add('nav-item');

        if (!model) {
            tabLink.innerHTML = `<a id="${tabId}" class="nav-link" data-bs-toggle="tab" href="#${contentId}" role="tab">
                                    checkpoint ${tabId.split(`-`)[0]}
                                    ${this.rowData.Manual ? `<span class="close-tab">&times;</span>`: ``}
                                </a>`
        } else {
            tabLink.innerHTML = `<a id="${tabId}" class="nav-link" data-bs-toggle="tab" href="#${contentId}" role="tab">
                                ${model}
                                <span class="close-tab">&times;</span>
                              </a>`;
        }
        
        return tabLink;

    }

    createTabContent(contentId, tabContentHTML) {
        const tabContent = document.createElement('div');
        tabContent.classList.add('tab-pane', 'fade');
        tabContent.id = contentId;
        tabContent.role = 'tabpanel';
        tabContent.innerHTML = tabContentHTML;
        return tabContent;
    }

    // setup content for created tab
    setupTabContent(contentType, id) {
        if (contentType === 'edit') {
            this.initializeEditTab(id);
        } else if (contentType === 'duplicate') {
            this.initializeDuplicateTab(id);
        } else {
            this.initializeCheckpointTab(id);
        }
    }

    initializeCheckpointTab(id) {
        this.addFormSubmitListener(id, 'checkpoint');
    }

    initializeEditTab(id) {
        const $select = this.initializeDynamicSelectize(id);
        $select[0].selectize.setValue(this.rowData.SupplierID);
        this.addFormSubmitListener(id, 'edit');
    }

    initializeDuplicateTab(id) {
        const qtyInput = {
            id,
            currentLotNumber: 1,
            savedLots: [],
            reqQtyEl: document.getElementById(`reqQty-${id}`),
            setQtyEl: document.getElementById(`setQty-${id}`),
            lotContainer: document.getElementById(`lot-container-${id}`),
        };
        qtyInputs.push(qtyInput);
        this.addLot(id, this.rowData);
        this.addFormSubmitListener(id, 'duplicate');
        document.getElementById(`addLot-${id}`).addEventListener('click', () => this.addLot(id, this.rowData));
    }

    showTab(tabLink) {
        const tabLinkEl = new bootstrap.Tab(tabLink.querySelector('a'));
        tabLinkEl.show();
    }

    addCloseTabListener(tabLink, tabId, contentId) {
        tabLink.querySelector('.close-tab').addEventListener('click', (e) => {
            e.stopPropagation();
            this.removeTab(tabId, contentId);
        });
    }

    addFormSubmitListener(id, type) {
        const formId = type === 'edit' ? `partDeliveryForm-${id}` : type === 'duplicate' ? `lot-forms-${id}` : `checkpointForm-${id}`;
        const form = document.getElementById(formId);
        if (form) {
            form.addEventListener('submit', (e) => {
                e.preventDefault();
                if (type === 'edit') {
                    this.editChanges(id, this.rowData.DeliveryDetailID, this.rowData.DeliveryDetailVersion, this.rowData.DeliveryVersion, `${id}-tab`, `${id}-content`, this.rowData.DeliveryID);
                } else if (type === 'duplicate') {
                    this.saveLotChanges(id, `${id}-tab`, `${id}-content`);
                } else {
                    this.saveCheckpoint(id, `${id}-tab`, `${id}-content`);
                }
            });
        }
    }

    removeTab(tabId, contentId) {
        const tabLink = document.getElementById(tabId).parentNode;
        const tabContent = document.getElementById(contentId);

        if (tabLink && tabContent) {
            tabLink.remove();
            tabContent.remove();
            if (this.rowData.Manual) DynamicTabs.tabCounter--;
            this.updateTabsAfterRemoval();
        }
    }

    updateTabsAfterRemoval() {
        if (this.tabLinksContainer.children.length > 0) {
            const firstTabLink = this.tabLinksContainer.querySelector('a');
            const firstTab = new bootstrap.Tab(firstTabLink);
            firstTab.show();
        } else {
            if (this.contentType === 'checkpoint') this.hideMainContainer();
            else this.hideRightColumn();
        }
    }

    hideMainContainer() {
        document.getElementById('checkpoint-container').classList.add('d-none');
        DynamicTabs.tabCounter = 0;
    }

    hideRightColumn() {
        document.getElementById('right-column').classList.add('d-none');
        const leftColumn = document.getElementById('left-column');
        leftColumn.classList.remove('col-lg-6', 'col-md-12');
        leftColumn.classList.add('col-lg-12');
    }

    initializeDynamicSelectize(id) {
        return $(`#Supplier-${id}`).selectize({
            valueField: 'Value',
            labelField: 'Text',
            searchField: 'Text',
            options: suppliersList,
            create: false,
        });
    }

    getHtmlContent(contentType, id, rowData, tabId, contentId) {
        const content = contentType.toLowerCase();
        const deliveryDetailId = `${rowData.DeliveryDetailID}`;
        const deliveryId = `${rowData.DeliveryID}`;
        const version = rowData.Version;
        const deliveryVersion = rowData.DeliveryVersion;

        switch (content) {
            case 'edit':
                return ` <form id="partDeliveryForm-${id}">
                            <div class="mb-3">
                                <label for="DateDelivered-${id}" class="form-label">Date Delivered</label>
                                <input type="date" class="form-control" id="DateDelivered-${id}" name="DateDelivered" value="${convertDateStringToFormattedString(rowData.DateDelivered)}" readonly>
                                                         
                            </div>

                            <div class="mb-3">
                                <label for="PartCode-${id}" class="form-label">Part Code</label>
                                <input type="text" class="form-control" id="PartCode-${id}" name="PartCode" value="${rowData.PartCode}" readonly>
                            </div>

                            <div class="mb-3">
                                <label for="PartName-${id}" class="form-label">Part Name</label>
                                <input type="text" class="form-control" id="PartName-${id}" name="PartName" value="${rowData.PartName}" readonly>
                            </div>

                            <div class="mb-3">
                                <label for="Model-${id}" class="form-label">Model</label>
                                <input type="text" class="form-control" id="Model-${id}" name="Model" value="${rowData.Model}" readonly>
                            </div>

                            <div class="mb-3">
                                <label for="DRNumber-${id}" class="form-label">DR Number</label>
                                <input type="text" class="form-control" id="DRNumber-${id}" name="DRNumber" value="${rowData.DRNumber}" readonly>
                            </div>

                            <div class="mb-3">
                                <label for="Supplier-${id}" class="form-label">Supplier</label>
                                <select type="text" class="form-control" id="Supplier-${id}" placeholder="Select a supplier" required></select>
                            </div>

                            <div class="mb-3">
                                <label for="TotalQuantity-${id}" class="form-label">Total Quantity</label>
                                <input type="number" class="form-control" id="TotalQuantity-${id}" name="TotalQuantity" value="${rowData.TotalQuantity}" required>
                            </div>
                            <div class="mb-3">
                                <label for="LotNumber-${id}" class="form-label">Lot Number</label>
                                <input type="text" class="form-control" id="LotNumber-${id}" name="LotNumber" value="${rowData.LotNumber != null ? `${rowData.LotNumber}` : `` }" required>
                            </div>
                            <div class="mb-3">
                                <label for="LotQuantity-${id}" class="form-label">Lot Quantity</label>
                                <input type="number" class="form-control" id="LotQuantity-${id}" name="LotQuantity" value="${rowData.LotQuantity}" required>
                            </div>
                            <button type="submit" class="btn btn-primary">Submit</button>
                        </form >`;

            case 'duplicate':
                return `<form id="lot-forms-${id}">
                            <div class="d-flex flex-md-row justify-content-between mb-3">
                                <div class="d-flex flex-md-row w-50 gap-2">
                                    <div class="d-flex flex-column w-50">
                                        <label for="reqQty-${id}" class="form-label">Required Qty</label>
                                        <input type="number" id="reqQty-${id}" class="form-control" value="${rowData.LotQuantity}" readonly />
                                    </div>
                                    <div class="d-flex flex-column w-50">
                                        <label for="setQty-${id}" class="form-label">Current Set Qty</label>
                                        <input type="number" id="setQty-${id}" class="form-control" value="${rowData.LotQuantity}" readonly />
                                    </div>
                                </div>
                                <div class="col-md-3 d-flex justify-content-end align-items-center gap-2 w-50">
                                    <button type="button" id="addLot-${id}" class="btn btn-primary addlot-btn")">Add Lot</button>
                                    <button type="submit" id="saveChanges" class="btn btn-success">Save</button>
                                </div>
                            </div>
                            <div id="lot-container-${id}" class="row"></div>
                        </form>`;
            case 'checkpoint':
                return `<form id="checkpointForm-${id}">
                            <div class="mb-3">
                                <label for="Code-${id}" class="form-label">Code</label>
                                <input type="text" class="form-control" id="Code-${id}" name="Code" value="${rowData.Code ? rowData.Code : ``}">
                                 <input type="hidden" id="PartID" value="${rowData.PartID ? rowData.PartID : ``}" name="PartID">
                            </div>

                            <div class="mb-3">
                                <label for="InspectionPart-${id}" class="form-label">Inspection Part</label>
                                <input type="text" class="form-control" id="InspectionPart-${id}" name="InspectionPart" value="${rowData.InspectionPart ? rowData.InspectionPart : ``}">
                            </div>

                            <div class="d-flex flex-lg-row flex-md-column flex-sm-column mb-3 gap-4">
                                <div>
                                    <label for="Specification-${id}" class="form-label">Specification</label>
                                    <input type="text" class="form-control" id="Specification-${id}" name="Specification" value="${rowData.Specification ? rowData.Specification : ``}">
                                    <input type="hidden" name="SpecificationRange" value="${rowData.SpecificationRange ? rowData.SpecificationRange : ``}">
                                </div>
                                <div>
                                    <label for="UpperLimit-${id}" class="form-label">Upper Limit</label>
                                    <input type="number" class="form-control" id="UpperLimit-${id}" name="UpperLimit" value="${rowData.UpperLimit ? rowData.UpperLimit : ``}">
                                </div>
                                <div>
                                    <label for="LowerLimit-${id}" class="form-label">Lower Limit</label>
                                    <input type="number" class="form-control" id="LowerLimit-${id}" name="LowerLimit" value="${rowData.LowerLimit ? rowData.LowerLimit : ``}">
                                </div>
                            </div>


                            <div class="mb-3">
                                <p class="form-label">Measurement?</p>
                                <div class="form-check custom-radio">
                                    <input class="form-check-input" type="radio" name="IsMeasurement" id="IsMeasurementT" value="true" checked/>
                                    <label class="form-check-label" for="IsMeasurementT">True</label>
                                </div>
                                <div class="form-check custom-radio">
                                    <input class="form-check-input" type="radio" name="IsMeasurement" id="IsMeasurementF" value="false"/>
                                    <label class="form-check-label" for="IsMeasurementF">False</label>
                                </div>
                            </div>

                            <div class="mb-3">
                                <label for="Tool-${id}" class="form-label">Tool</label>
                                <input type="text" class="form-control" id="Tool-${id}" name="Tool" value="${rowData.Tool ? rowData.Tool : ``}">
                            </div>
                            <div class="mb-3">
                                <label for="MethodSampling-${id}" class="form-label">Sampling Method</label>
                                <input type="text" class="form-control" id="MethodSampling-${id}" name="MethodSampling" value="${rowData.MethodSampling ? rowData.MethodSampling : ``}">
                            </div>
                            <div class="d-flex flex-lg-row flex-md-column flex-sm-column mb-3 gap-4">
                                <div>
                                    <label for="Level-${id}" class="form-label">Level</label>
                                    <input type="text" class="form-control" id="Level-${id}" name="Level" value="${rowData.Level ? rowData.Level : ``}">
                                </div>
                                <div>
                                    <label for="LevelNum-${id}" class="form-label">LevelNum</label>
                                    <input type="text" class="form-control" id="LevelNum-${id}" name="LevelNum" value="${rowData.Level_1 ? rowData.Level_1 : ``}">
                                </div>
                            </div>
                            <div class="mb-3">
                                <label for="Note-${id}" class="form-label">Note</label>
                                <input type="text" class="form-control" id="Note-${id}" name="Note" value="${rowData.Note ? rowData.Note : ``}">
                            </div>
                            <button type="submit" class="btn btn-primary">Submit</button>
                        </form >`
            default:
                return `<p>No Content Inserted</p>`;
        }
    }

    // For Refreshing DataTables
    reloadDataTables(dataTables) {
        console.log('table reloaded');
        dataTables.forEach(table => table.ajax.reload());
    }

    // For Saving Checkpoint Functionality
    saveCheckpoint(id, tabId, contentId) {
        
        const form = document.getElementById(`checkpointForm-${id}`);
        const formData = new FormData(form);
        const { hasError: { status, message }, data } = this.formDataValidatorClass(formData, this.contentType);
        console.log(Object.fromEntries(data));
        if (status) {
            alertify.error(message);
        } else {
            $.ajax({
                url: `/Checkpoints/UploadCheckpoint`,
                type: 'POST',
                data: Object.fromEntries(data),
                success: (res) => {
                    console.log(res);
                    this.removeTab(tabId, contentId);

                },
                error: (err) => {
                    console.log(err);
                }
            });
        }
    }

    // For Edit Functionality
    editChanges(id, deliveryDetailId, version, deliveryVersion, tabId, contentId, deliveryId) {
        console.log(id, deliveryDetailId, version, deliveryVersion, tabId, contentId, deliveryId);
        const totalQty = Math.abs(parseInt(document.getElementById(`TotalQuantity-${id}`).value));
        const lotQty = Math.abs(parseInt(document.getElementById(`LotQuantity-${id}`).value));
        if (totalQty >= lotQty) {
            const dynamicForm = document.getElementById(`partDeliveryForm-${id}`);
            const supplierSelected = document.getElementById(`Supplier-${id}`).selectize.getValue();
            const formData = new FormData(dynamicForm);
            formData.append("Supplier", supplierSelected);
            formData.append("DeliveryDetailId", deliveryDetailId);
            formData.append("DeliveryId", deliveryId);
            formData.append("DeliveryDetailVersion", version);
            formData.append("DeliveryVersion", deliveryVersion);
            const { hasError: { status, message }, data: newData } = this.formDataValidatorClass(formData, this.contentType)
            const data = Object.fromEntries(newData);
            data['LotQuantity'] = Math.abs(parseInt(data['LotQuantity']));
            data['TotalQuantity'] = Math.abs(parseInt(data['TotalQuantity']));
            console.log(data);

            if (status) {
                alertify.error(message);
            } else {
                $.ajax({
                    url: '/Scheduling/EditDelivery',
                    type: 'POST',
                    data,
                    success: (res) => {
                        if (res.Success) alertify.success(`${res.Status}: ${res.Message}`);
                        else alertify.error(`${res.Status}: ${res.Message}`);
                        this.reloadDataTables(this.dataTables);
                        this.removeTab(tabId, contentId);
                    },
                    error: (err) => {
                        alertify.error(`${err}`);
                    }
                });
            }
            
        } else {
            console.log(totalQty, lotQty);
            alertify.error(`The Quantity in the lot exceeded the total quantity.`)
        }

    }

    // For Adding Multiple Rows
    addLot(id, rowData) {
        let data;
        if (typeof rowData === "string") data = JSON.parse(decodeURIComponent(rowData));
        else data = rowData;
        let { reqQtyEl, lotContainer } = qtyInputs.find(input => input.id === id);
        const rowObj = qtyInputs.find(input => input.id === id);
        let currentLotNumber = rowObj.currentLotNumber++;
        const reqQty = parseInt(reqQtyEl.value);
        const newQty = Math.floor(reqQty / currentLotNumber);
        const identifier = `${id}-${currentLotNumber}`;
        const newLotEl = document.createElement('div');
        newLotEl.innerHTML = this.createLotHTML(currentLotNumber, newQty, id, data);
        lotContainer.appendChild(newLotEl.firstElementChild);
        this.redistributeQuantities(id);
    }

    removeLot(lotNumber, id) {
        const rowObj = qtyInputs.find(input => input.id === id);
        let currentLotNumber = rowObj.currentLotNumber--;
        document.getElementById(`lotInfo-${lotNumber}-${id}`).remove();
        this.redistributeQuantities(id);
    }

    redistributeQuantities(id) {
        let { reqQtyEl, setQtyEl } = qtyInputs.find(input => input.id === id);
        const reqQty = parseInt(reqQtyEl.value);
        const lotQtyInputs = document.querySelectorAll(`.lot-qty-${id}`);

        if (lotQtyInputs.length === 0) {
            setQtyEl.value = reqQty;
            return;
        }

        const baseQty = Math.floor(reqQty / lotQtyInputs.length);
        let remainder = reqQty % lotQtyInputs.length;

        lotQtyInputs.forEach((input, index) => {
            let qty = baseQty;
            if (remainder > 0) {
                qty++;
                remainder--;
            }
            input.value = qty;
        });

        this.updateSetQty(id);
    }

    updateSetQty(id) {
        let { setQtyEl } = qtyInputs.find(input => input.id === id);
        const lotQtyInputs = document.querySelectorAll(`.lot-qty-${id}`);
        let totalQty = 0;

        lotQtyInputs.forEach(input => {
            totalQty += parseInt(input.value) || 0;
        });

        setQtyEl.value = totalQty;
    }

    createLotHTML(lotNumber, qty, id, data) {
        return `
                <div id="lotInfo-${lotNumber}-${id}" class=" col-lg-6 col-md-12 mb-3">
                    <div class="card text-white bg-secondary">
                        <div class="card-body">
                            <div class="d-flex justify-content-md-end">
                                ${lotNumber !== 1 ? `<button type="button" class="btn btn-danger btn-sm float-end mb-1" onclick="removeLot(${lotNumber},'${id}')">Remove</button>` : ``}
                                <input type="hidden" class="version-${id}" value="${data.DeliveryDetailVersion}"/>
                                <input type="hidden" class="deliveryVersion-${id}" value="${data.DeliveryVersion}" />
                                <input type="hidden" class="deliveryId-${id}" value="${data.DeliveryID}"/>
                                <input type="hidden" class="deliveryDetailId-${id}" value="${data.DeliveryDetailID}"/>
                            </div>
                            <div class="row">
                                <div class="col-md-6">
                                    <div class="mb-2">
                                        <label for="dateDelivered-${lotNumber}" class="form-label">Date Delivered</label>
                                        <input type="text" id="dateDelivered-${lotNumber}" class="form-control dateDelivered-${id}" value="${convertDateStringToFormattedString(data.DateDelivered)}" readonly />
                                    </div>
                                    <div class="mb-2">
                                        <label for="partCode-${lotNumber}" class="form-label">Part Code</label>
                                        <input type="text" id="partCode-${lotNumber}" class="form-control partCode-${id}" value="${data.PartCode}" readonly />
                                    </div>
                                    <div class="mb-2">
                                        <label for="part-${lotNumber}" class="form-label">Part Name</label>
                                        <input type="text" id="part-${lotNumber}" class="form-control part-name-${id}" value="${data.PartName}" readonly />
                                    </div>
                                    <div class="mb-2">
                                        <label for="model-${lotNumber}" class="form-label">Model</label>
                                        <input type="text" id="model-${lotNumber}" class="form-control model-${id}" value="${data.Model}" readonly />
                                    </div>
                                    <div class="mb-2">
                                        <label for="drNum-${lotNumber}" class="form-label">DR Number</label>
                                        <input type="text" id="drNum-${lotNumber}" class="form-control drNum-${id}" value="${data.DRNumber}" readonly />
                                    </div>
                                </div>
                                <div class="col-md-6">
                                    <div class="mb-2">
                                        <label for="Supplier-${id}-${lotNumber}" class="form-label">Supplier</label>
                                        <input id="Supplier-${id}-${lotNumber}" class="form-control supplier-${id}" value="${data.Supplier}" readonly/>
                                        <input type="hidden" id="SupplierID-${id}-${lotNumber}" class="form-control supplierId-${id}" value="${data.SupplierID}" />
                                    </div>
                                    <div class="mb-2">
                                        <label for="qty-${lotNumber}" class="form-label">Total Quantity</label>
                                        <input type="number" id="qty-${lotNumber}" class="form-control total-qty-${id}" value="${data.TotalQuantity}" readonly/>
                                    </div>
                                    <div class="mb-2">
                                        <label for="lot-${lotNumber}" class="form-label">Lot Number</label>
                                        <input type="text" id="lot-${lotNumber}" class="form-control lot-code-${id}" value="${lotNumber !== 1 ? `` : data.LotNumber != null ? `${data.LotNumber}` : ``}" required/>
                                    </div>
                                    <div class="mb-2">
                                        <label for="lotQty-${lotNumber}" class="form-label">Lot Quantity</label>
                                        <input type="number" id="lotQty-${lotNumber}" class="form-control lot-qty-${id}" value="${qty}" onchange="updateSetQty('${id}')" required/>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            `;
    }

    saveLotChanges(id, tabId, contentId) {
        const reqQty = document.getElementById(`reqQty-${id}`);
        const setQty = document.getElementById(`setQty-${id}`);
        let hasNoLot = false;
        let hasNoQuantity = false;
        if (parseInt(reqQty.value) !== parseInt(setQty.value)) {
            alertify.error('Current set quantity is not equal to the required quantity!');
        } else {
            const savedLots = qtyInputs.find(input => input.id === id).savedLots;
            const lots = document.querySelectorAll(`#lot-container-${id} > div`);
            lots.forEach((lot, index) => {
                let trimmedLot = lot.querySelector(`.lot-code-${id}`).value.trim();
                let identifiedQuantity = Number(lot.querySelector(`.lot-qty-${id}`).value);
                if (trimmedLot === '') hasNoLot = true;
                if (identifiedQuantity === 0 || identifiedQuantity === NaN) hasNoQuantity = true;
                if (index === 0) savedLots.push({
                    DeliveryID: lot.querySelector(`.deliveryId-${id}`).value,
                    DeliveryDetailID: lot.querySelector(`.deliveryDetailId-${id}`).value,
                    DateDelivered: lot.querySelector(`.dateDelivered-${id}`).value,
                    PartCode: lot.querySelector(`.partCode-${id}`).value,
                    PartName: lot.querySelector(`.part-name-${id}`).value,
                    Model: lot.querySelector(`.model-${id}`).value,
                    DRNumber: lot.querySelector(`.drNum-${id}`).value,
                    Supplier: lot.querySelector(`.supplierId-${id}`).value,
                    TotalQuantity: parseInt(lot.querySelector(`.total-qty-${id}`).value) || 0,
                    LotNumber: lot.querySelector(`.lot-code-${id}`).value,
                    LotQuantity: lot.querySelector(`.lot-qty-${id}`).value || 0,
                    DeliveryDetailVersion: lot.querySelector(`.version-${id}`).value,
                    DeliveryVersion: lot.querySelector(`.deliveryVersion-${id}`).value
                });
                else savedLots.push({
                    DateDelivered: lot.querySelector(`.dateDelivered-${id}`).value,
                    DeliveryID: lot.querySelector(`.deliveryId-${id}`).value,
                    PartCode: lot.querySelector(`.partCode-${id}`).value,
                    PartName: lot.querySelector(`.part-name-${id}`).value,
                    Model: lot.querySelector(`.model-${id}`).value,
                    DRNumber: lot.querySelector(`.drNum-${id}`).value,
                    Supplier: lot.querySelector(`.supplierId-${id}`).value,
                    TotalQuantity: parseInt(lot.querySelector(`.total-qty-${id}`).value) || 0,
                    LotNumber: lot.querySelector(`.lot-code-${id}`).value,
                    LotQuantity: lot.querySelector(`.lot-qty-${id}`).value || 0,
                    DeliveryVersion: lot.querySelector(`.deliveryVersion-${id}`).value
                });
            });
            const [firstLot, ...otherLots] = savedLots;
            if (otherLots.length <= 0) {
                alertify.error("Add lot/s to duplcate item");
                savedLots.splice(0, savedLots.length);
            } else if (hasNoLot) {
                alertify.error("Please assign a Lot Number to unfilled forms");
                savedLots.splice(0, savedLots.length);
            } else if (hasNoQuantity) {
                alertify.error("Unable to save lots with quantity of 0");
                savedLots.splice(0, savedLots.length);
            } else {
                $.ajax({
                    url: '/Scheduling/DuplicateDelivery',
                    type: 'POST',
                    data: {
                        firstLot,
                        otherLots
                    },
                    success: (res) => {
                        if (res.Success) alertify.success(`${res.Status}: ${res.Message}`);
                        else alertify.error(`${res.Status}: ${res.Message}`);
                        this.reloadDataTables(this.dataTables);
                        this.removeTab(tabId, contentId);
                    },
                    error: (err) => {
                        alertify.error(`${err}`);
                    }
                });
                // Clear Lots After Saving
                savedLots.splice(0, savedLots.length);
            }
        }
    }

    // FORM VALIDATOR FOR FORM SUBMISSIONS
    formDataValidatorClass(formData, contentType) {
        const data = new FormData();
        let hasError = { status: false, message: "" };
        formData.forEach((value, key) => {
            if (contentType === "checkpoint") {
                if (key === "SpecificationRange") value = value === '' ? "none" : value;
                if (key === "UpperLimit" || key === "LowerLimit") value = value === '' ? 0 : Number(value);
                data.append(key, value)
            }

            if (typeof value === "string") {
                value = value.trim();
                if (value === '') {
                    hasError.status = true;
                    hasError.message = "Avoid the use of white spaces ";
                }
                data.append(key, value);
            }
        });

        if (contentType === "checkpoint") {
            const upperLimit = Number(formData.get('UpperLimit'));
            const lowerLimit = Number(formData.get('LowerLimit'));

            if (upperLimit < lowerLimit) {
                hasError.status = true;
                hasError.message += "Upper limit cannot be lower than the Lower limit ";
            }
        }


        return { hasError, data };
    }
}