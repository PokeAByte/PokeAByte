class GameHookProperty {
    _client = null

    path = null
    type = null
    memoryContainer = null
    address = null
    length = null
    size = null
    bits = null
    reference = null
    description = null

    value = null
    bytes = null
    isFrozen = null
    isReadOnly = null

    constructor(client, obj) {
        this._client = client

        for (const item of Object.entries(obj)) {
            this[item[0]] = item[1]
        }
    }

    async set(value, freeze) { this._client._editPropertyValue(this.path, value, freeze) }
    async setBytes(bytes, freeze) { this._client._editPropertyBytes(this.path, bytes, freeze) }

    async freeze(freeze = true) {
        this._client._editPropertyBytes(this.path, this.bytes, freeze)
    }

    change(fn) {
        if (!this._client._change[this.path]) {
            this._client._change[this.path] = []
        }

        this._client._change[this.path].push(fn)
    }

    once(fn) {
        if (!this._client._once[this.path]) {
            this._client._once[this.path] = []
        }

        this._client._once[this.path].push(fn)
    }

    toString() {
        if (this.value === undefined || this.value === null) { return null }

        return this.value.toString()
    }
}

class GameHookMapperClient {
    _connectionString
    _signalrClient
    _properties

    meta
    properties
    glossary

    connected = false

    get mapperLoaded() {
        return !meta ? false : true
    }

    get _signalrConnectionEstablished() {
        return this._signalrClient != null && this._signalrClient.connection.q === 'Connected'
    }

    _change = []
    _once = []

    static decimalToHexdecimal(x, uppercase = true) {
        if (x == null) return null

        let stringValue = x.toString(16)

        // If the string is of odd length, we
        // need to introduce a leading zero.
        if (stringValue.length % 2) {
            stringValue = '0' + stringValue
        }

        if (uppercase) return stringValue.toUpperCase()
        else return stringValue
    }

    static hexdecimalToDecimal(x) {
        if (x == null) return null
        return parseInt(x, 16)
    }

    constructor(connectionString = 'http://localhost:8085') {
        this._connectionString = connectionString

        this._options = {
            automaticRefreshMapperTimeMinutes: 1
        }
    }

    _deconstructMapper() {
        this.meta = null
        this._properties = null
        this.properties = null
        this.glossary = null
    }

    get(path) {
        return this._properties.find(x => x.path === path)
    }

    async loadMapper() {
        console.debug('[GameHook Client] Loading mapper.')

        function assign(final, path, value) {
            let lastKeyIndex = path.length - 1

            for (var i = 0; i < lastKeyIndex; ++i) {
                let key = path[i]
                if (!(key in final)) {
                    final[key] = /^\d+$/.test(path[i + 1]) ? [] : {}
                }

                final = final[key]
            }

            final[path[lastKeyIndex]] = value
        }

        let mapper = await fetch(`${this._connectionString}/mapper`)
            .then(async (x) => {
                return { response: x, body: await x.json() }
            })
            .then(x => {
                if (x.response.status === 200) {
                    return x.body
                } else {
                    this._deconstructMapper()

                    if (x.body) {
                        throw x.body
                    } else {
                        throw new Error('Unknown error.')
                    }
                }
            })

        this.meta = mapper.meta
        this.glossary = mapper.glossary

        // Translate properties from a flat array to a nested object.
        this.properties = {}
        this._properties = mapper.properties.map(x => new GameHookProperty(this, x))
        this._properties.forEach(x => assign(this.properties, x.path.split('.'), x))

        setTimeout(() => this.loadMapper(), this._options.automaticRefreshMapperTimeMinutes * 60000)

        return this
    }

    async _establishConnection() {
        try {
            if (this._signalrConnectionEstablished == false) {
                await this._signalrClient.start()
                console.debug('[GameHook Client] GameHook successfully established a SignalR connection.')
            }

            // Load the data from the server.
            await this.loadMapper()

            this.connected = true
            this.onConnected()
            console.debug('[GameHook Client] GameHook is now connected.')

            return true
        } catch (err) {
            this._deconstructMapper()

            console.error(err)
            this.onMapperLoadError(err)

            setTimeout(() => this._establishConnection(), 5000)

            return false
        }
    }

    async connect() {
        var that = this

        this._signalrClient = new signalR.HubConnectionBuilder()
            .withUrl(`${this._connectionString}/updates`)
            .configureLogging(signalR.LogLevel.Warning)
            .build()

        this._signalrClient.onclose(async () => {
            console.debug('[GameHook Client] SignalR connection lost. Attempting to reconnect...')

            this._deconstructMapper()

            this.onDisconnected()
            this.connected = false

            await this._establishConnection()
        })

        this._signalrClient.on('PropertiesChanged', (propertiesChanged) => {
            if (that._properties && that._properties.length > 0) {
                for (const propertyChanged of propertiesChanged) {
                    let property = that._properties.find(x => x.path === propertyChanged.path)
                    if (!property) {
                        console.warn(`[GameHook Client] Could not find a related property in PropertyUpdated event for: ${propertyChanged.path}`)
                        return
                    }

                    let oldProperty = {
                        path: property.path,
                        memoryContainer: property.memoryContainer,
                        address: property.address,
                        length: property.length,
                        size: property.size,
                        reference: property.reference,
                        bits: property.bits,
                        description: property.description,
                        value: property.value,
                        bytes: property.bytes,
                        isFrozen: property.frozen,
                        isReadOnly: property.isReadOnly
                    }

                    property.memoryContainer = propertyChanged.memoryContainer
                    property.address = propertyChanged.address
                    property.length = propertyChanged.length
                    property.size = propertyChanged.size
                    property.reference = propertyChanged.reference
                    property.bits = propertyChanged.bits
                    property.description = propertyChanged.description
                    property.value = propertyChanged.value
                    property.bytes = propertyChanged.bytes
                    property.isFrozen = propertyChanged.isFrozen
                    property.isReadOnly = propertyChanged.isReadOnly

                    // Only trigger the property's change events when
                    // the value has changed.

                    // This is functionally 'weird', but users are really
                    // only interested in when the value changed.

                    // If they need to know about other fields changing,
                    // they can register to the global GameHook event handler.

                    if (propertyChanged.fieldsChanged.includes('value')) {
                        // Trigger the property.change events if any.
                        const changeArray = that._change[property.path]
                        if (changeArray && changeArray.length > 0) {
                            changeArray.forEach(x => {
                                x(property, oldProperty)
                            })
                        }

                        // Trigger the property.once events if any.
                        const onceArray = that._once[property.path]
                        if (onceArray && onceArray.length > 0) {
                            onceArray.forEach(x => {
                                x(property, oldProperty)
                            })

                            that._once[property.path] = []
                        }
                    }

                    // Trigger the global property changed event.
                    if (that.onPropertyChanged) {
                        that.onPropertyChanged(property, oldProperty, propertyChanged.fieldsChanged)
                    }
                }
            } else {
                console.debug('[GameHook Client] Mapper is not loaded, throwing away PropertiesChanged event.')
            }
        })

        this._signalrClient.on('MapperLoaded', async () => { await this.loadMapper(); this.onMapperLoaded() })
        this._signalrClient.on('GameHookError', (err) => { this.onGameHookError(err) })
        this._signalrClient.on('DriverError', (err) => { this.onDriverError(err) })
        this._signalrClient.on('SendDriverRecovered', () => { this.onDriverRecovered() })
        this._signalrClient.on('UiBuilderScreenSaved', (id) => { this.onUiBuilderScreenSaved(id) })

        return (await this._establishConnection())
    }

    async _editPropertyValue(path, value, freeze) {
        path = path.replace('.', '/')

        await fetch(`${this._connectionString}/mapper/set-property-value/`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ path, value, freeze })
        })
            .then(async (x) => { return { response: x } })
            .then(x => {
                if (x.response.status === 200) {
                    return
                } else {
                    if (x.body) {
                        throw new Error(x.body)
                    } else {
                        throw new Error('Unknown error')
                    }
                }
            })
    }

    async _editPropertyBytes(path, bytes, freeze) {
        path = path.replace('.', '/')

        await fetch(`${this._connectionString}/mapper/set-property-bytes/`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ path, bytes, freeze })
        })
            .then(async (x) => { return { response: x } })
            .then(x => {
                if (x.response.status === 200) {
                    return
                } else {
                    if (x.body) {
                        throw new Error(x.body)
                    } else {
                        throw new Error('Unknown error')
                    }
                }
            })
    }

    onConnected() { /* Override this with your own function. */ }
    onDisconnected() { /* Override this with your own function. */ }

    onGameHookError(err) { /* Override this with your own function. */ }
    onMapperLoaded() { /* Override this with your own function. */ }
    onMapperLoadError(err) { /* Override this with your own function. */ }
    onDriverError(err) { /* Override this with your own function. */ }
    onPropertyChanged(property, oldProperty, fieldsChanged) { /* Override this with your own function. */ }

    onUiBuilderScreenSaved(id) { /* Override this with your own function. */ }
}