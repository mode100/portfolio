declare module "" {
    import Phaser, { GameObjects } from "phaser"
    import MT from "mersennetwister"
    import Cookies from "js-cookie"
}

declare class MersenneTwister {
    /** Creates a new instance seeded by an unsined 32-bit integer or or array of unsigned 32-bit integers */
    constructor(seed?: number | readonly number[]);
    /** Returns a random 32-bit unsigned integer */
    int(): number;
    /** Returns a random 31-bit unsigned integer */
    int31(): number;
    /** Returns a random float in the range [0,1) with 32-bit precision */
    rnd(): number;
    /**
     * This is an alias of the `rnd()` method
     * @alias rnd()
     */
    random(): number;
    /** Returns a random float in the range [0,1) with 53-bit precision */
    rndHiRes(): number;
    /** Returns a random float in the range [0,1] */
    real(): number;
    /** Returns a random float in the range (0,1) */
    realx(): number;
    /** Reseed the generator with the specified 32-bit unsigned integer */
    seed(seed: number): void;
    /** Reseed the generator's state vector with an array of 32-bit unsigned integers */
    seedArray(seeds: readonly number[]): void;
}


class MyMath
{
    static create2dArray(len1:integer,len2:integer,value:any=0):any
    {
        return new Array(len1).fill(0).map(() => new Array(len2).fill(value));
    }
}

const DEBUG_MODE = false

enum PageIndex
{
    CREATE,
    BATTLE,
    CHALLENGE,
    RANKBATTLE,
    MEMO,
    DEBUG,
    CHALLENGEBATTLE = 6,
}

enum InputState
{
    NOSIGNAL,
    DOWN,
    JUSTDOWN,
    UP,
}

class Input
{
    static game : Phaser.Scene
    static get mouse_x()
    {
        return Input.game.input.mousePointer.x
    }
    static get mouse_y()
    {
        return Input.game.input.mousePointer.y
    }
    static mouse_state:InputState = 0

    static init(game:Phaser.Scene)
    {
        Input.game = game
        game.input.on("pointerdown",(pointer:any) => {
            Input.mouse_state = InputState.JUSTDOWN
        },game)
        game.input.on("pointerup",(pointer:any) => {
            Input.mouse_state = InputState.UP
        })
    }

    static update()
    {
        if (Input.mouse_state == InputState.JUSTDOWN)
        {
            Input.mouse_state = InputState.DOWN
        }
        if (Input.mouse_state == InputState.UP)
        {
            Input.mouse_state = InputState.NOSIGNAL
        }
    }

    static is_mouse_down()
    {
        return Input.mouse_state == InputState.DOWN || Input.is_mouse_just_down()
    }

    static is_mouse_just_down()
    {
        return Input.mouse_state == InputState.JUSTDOWN
    }

    static is_mouse_up()
    {
        return Input.mouse_state == InputState.UP
    }
}

enum Layer
{
    ALLY,
    ENEMY,
    ALLYBULLET,
    ENEMYBULLET,
}

class AbstractBattleScene extends Phaser.Scene implements IBattleField
{
    static scene: AbstractBattleScene

    static g:Phaser.GameObjects.Graphics

    allies: Character[] = []
    enemies: Character[] = []
    layers:Phaser.Physics.Arcade.Group[] = []

    // effects: (Effect|NumberUI)[] = []
    effects2: {[key:string]:Effect|NumberUI} = {}

    static objects: {[key:string]:Character|Bullet} = {}


    constructor(config:string | Phaser.Types.Scenes.SettingsConfig)
    {
        super(config)
        
    }

    update()
    {
        AbstractBattleScene.g.clear()
    }

    create()
    {
        AbstractBattleScene.g = this.add.graphics()
        AbstractBattleScene.scene = this
        AbstractBattleScene.objects = {}
        // this.effects = []
        this.effects2 = {}
        this.initLayers()
    }

    getOpponents(character:Character)
    {
        if (this.allies.includes(character))
        {
            return this.enemies
        }
        if (this.enemies.includes(character))
        {
            return this.allies
        }
        console.error("BattleScene.getOpponentsでキャラが味方でも敵でも無い")
        return []
    }

    getAllies(character:Character)
    {
        if (this.allies.includes(character))
        {
            return this.allies
        }
        if (this.enemies.includes(character))
        {
            return this.enemies
        }
        console.error("BattleScene.getOpponentsでキャラが味方でも敵でも無い")
        return []
    }

    static getAllBattlers()
    {
        return AbstractBattleScene.scene.allies.concat(AbstractBattleScene.scene.enemies)
    }
    
    static getAllLivingBattlers()
    {
        return AbstractBattleScene.getAllBattlers().filter(o=>o.hp>0)
    }

    initLayers()
    {
        this.layers = []
        for (let i = 0; i < 4; i++)
        {
            this.layers.push(this.physics.add.group())
        }
        this.physics.add.collider(this.layers[Layer.ALLY],this.layers[Layer.ALLY],collided)
        this.physics.add.collider(this.layers[Layer.ENEMY],this.layers[Layer.ENEMY],collided)
        this.physics.add.collider(this.layers[Layer.ALLY],this.layers[Layer.ENEMY],collided)
        this.physics.add.overlap(this.layers[Layer.ALLYBULLET],this.layers[Layer.ENEMY],collided)
        this.physics.add.overlap(this.layers[Layer.ALLY],this.layers[Layer.ENEMYBULLET],collided)
        // this.physics.add.collider(this.layers[Layer.ALLYBULLET],this.layers[Layer.ENEMYBULLET],collided)

    }

    addEffect(o:Effect|NumberUI)
    {
        o.name = Phaser.Utils.String.UUID()
        this.effects2[o.name] = o
    }

    removeEffect(o:Effect|NumberUI)
    {
        delete this.effects2[o.name]
    }

    setLayer(o:Character|Bullet)
    {
        if (o instanceof Character)
        {
            if (o.currentTeam == Team.ALLY)
            {
                this.layers[Layer.ALLY].add(o.container)
            }
            else if (o.currentTeam == Team.ENEMY)
            {
                this.layers[Layer.ENEMY].add(o.container)
            }
            o.container.name = Phaser.Utils.String.UUID()
            AbstractBattleScene.objects[o.container.name] = o

        }
        else if (o instanceof Bullet)
        {
            if (o.owner.currentTeam == Team.ALLY)
            {
                this.layers[Layer.ALLYBULLET].add(o.sprite)
            }
            else if (o.owner.currentTeam == Team.ENEMY)
            {
                this.layers[Layer.ENEMYBULLET].add(o.sprite)
            }
            o.sprite.name = Phaser.Utils.String.UUID()
            AbstractBattleScene.objects[o.sprite.name] = o
        }
    }


}

function calcMagAttackPoint(o:Character): integer
{
    let st = o.getModifiedStatus()
    return DamageObject.calcPoint(st.mag)
}

function attack(o1:Bullet,o2:Character)
{
    let dmgObj = new DamageObject(o1,o2)
    dmgObj.setCalcDmg()
    dmgObj.dmg()
}

function collided(_o1:Phaser.Types.Physics.Arcade.GameObjectWithBody|Phaser.Tilemaps.Tile,_o2:Phaser.Types.Physics.Arcade.GameObjectWithBody|Phaser.Tilemaps.Tile)
{
    if(_o1 instanceof Phaser.Tilemaps.Tile) return
    if(_o2 instanceof Phaser.Tilemaps.Tile) return
    let o1 = AbstractBattleScene.objects[_o1.name]
    let o2 = AbstractBattleScene.objects[_o2.name]
    if (o1 instanceof Bullet && o2 instanceof Character) attack(o1,o2)
    if (o2 instanceof Bullet && o1 instanceof Character) attack(o2,o1)
}

class CreatePage extends AbstractBattleScene
{
    static self: CreatePage

    g?: Phaser.GameObjects.Graphics

    static bars: {[key:string]:StatusBar} = {}

    static form? : HTMLInputElement
    static enterInputButton? : HTMLButtonElement

    static texts : {[key:string]:StatusText} = {}

    // static bar_maxs: {[key:string]:number} = {}

    static character: Character

    constructor()
    {
        super({key:getSceneName(PageIndex.CREATE)});     
    }

    preload()
    {

    }

    create()
    {
        super.create()
        CreatePage.self = this

        this.g = this.add.graphics()

        CreatePage.bars.hp = new StatusBar(this,"HP",280,0,4000,0x00ff00)
        CreatePage.bars.mp = new StatusBar(this,"MP",280+24,0,1000,0x0000ff)
        CreatePage.bars.atk = new StatusBar(this,"ATK",280+24*2,0,1000,0xff0000)
        CreatePage.bars.def = new StatusBar(this,"DEF",280+24*3,0,1000,0xcccccc)
        CreatePage.bars.spd = new StatusBar(this,"SPD",280+24*4,0,1000,0x42f5e0)
        CreatePage.bars.mag = new StatusBar(this,"MAG",280+24*5,0,1000,0xcc0088)
        CreatePage.bars.mdef = new StatusBar(this,"MND",280+24*6,0,1000,0x8800cc)

        CreatePage.texts.moveAI = new StatusText(this,"移動AI",424+24,"ー")
        CreatePage.texts.actionTargetAI = new StatusText(this,"優先対象",424+24*2,"ー")
        CreatePage.texts.actionAI = new StatusText(this,"戦闘AI",424+24*3,"ー")
        CreatePage.texts.actions1 = new StatusText(this,"スキル1",424+24*4,"ー")
        CreatePage.texts.actions2 = new StatusText(this,"スキル2",424+24*5,"ー")
        CreatePage.texts.actions3 = new StatusText(this,"スキル3",424+24*6,"ー")


        CreatePage.enterInput()
    }

    update()
    {

    }

    // ここはダミー用。使わない
    getOpponents(character: Character): Character[] {
        return []
    }

    static createHTML()
    {
        let page = HTML.getPageElement(PageIndex.CREATE)
        {
            let o = HTML.createHTMLElement("input",240,120,400,64,page) as HTMLInputElement
            CreatePage.form = o
            o.addEventListener("keydown",(e)=>{
                if (e.key == "Enter")
                {
                    CreatePage.enterInput()
                }
            })
        }

        let o2 = HTML.createHTMLElement("button",360,200,160,64,page) as HTMLButtonElement
        o2.innerText = "決定"
        o2.onclick = CreatePage.enterInput
        CreatePage.enterInputButton = o2

    }

    static enterInput()
    {
        if (!CreatePage.form) return
        if (CreatePage.form.value.length == 0) return
        // let status = createStatusFromName(CreatePage.form.value)
        let o = new Character(CreatePage.self,CreatePage.form.value)
        let st = o.getModifiedStatus()
        CreatePage.bars.hp.setValue(st.mhp)
        CreatePage.bars.mp.setValue(st.mmp)
        CreatePage.bars.atk.setValue(st.atk)
        CreatePage.bars.def.setValue(st.def)
        CreatePage.bars.spd.setValue(st.spd)
        CreatePage.bars.mag.setValue(st.mag)
        CreatePage.bars.mdef.setValue(st.mdef)
        CreatePage.texts.moveAI.setText(o.moveAI.name)
        CreatePage.texts.actionTargetAI.setText(o.actionTargetAI.name)
        CreatePage.texts.actionAI.setText(o.actionAI.name)
        CreatePage.texts.actions1.setText(o.actions[1].name)
        CreatePage.texts.actions2.setText(o.actions[2].name)
        CreatePage.texts.actions3.setText(o.actions[3].name)
        if (CreatePage.character)
        {
            CreatePage.character.destroy()
        }
        CreatePage.character = o
        o.setPosition(100,200)


    }

    static setBarLength(bar:Phaser.GameObjects.Rectangle, val:number, max:number)
    {
        let percent = Math.min(val/max,1)
        let w = (480-32*4)*percent
        bar.width = w
    }
}

class StatusBar
{
    rectangle: Phaser.GameObjects.Rectangle
    // name = "名前"
    val = 0
    valueText: Phaser.GameObjects.Text
    max = 0

    constructor(scene:Phaser.Scene,name:string,y:number,val:number,max:number,color:number)
    {
        this.max = max
        let w = 480-32*4
        let rect = scene.add.rectangle(96,y,w,16,color)
        
        rect.setOrigin(0,1/2)
        this.rectangle = rect
        let text = scene.add.text(32,y,name,{font:"24px sans-serif",color:"#000000"})
        text.setOrigin(0,1/2)

        this.valueText = scene.add.text(480-32,y,"ー",{font:"24px sans-serif",color:"#000000"})
        this.valueText.setOrigin(1,1/2)

        this.setValue(val)
    }

    setValue(val:number)
    {
        this.val = val
        let percent = Math.min(val/this.max,1)
        this.rectangle.setScale(percent,1)
        this.valueText.text = this.val.toString()
    }
}

class StatusText
{
    name = "名前"
    text: Phaser.GameObjects.Text

    constructor(scene:Phaser.Scene,name:string,y:number,val:string)
    {
        this.name = name
        this.text = scene.add.text(32,y,`${name}： ${val}`,{font:"24px sans-serif",color:"#000000"})
        this.text.setOrigin(0,1/2)

        this.setText(val)
    }

    setText(val:string)
    {
        this.text.setText(`${this.name}： ${val}`)
    }
}

enum Team
{
    NONE,
    ALLY,
    ENEMY,
}

class Character
{
    scene: AbstractBattleScene
    container: Phaser.GameObjects.Container
    name:string = ""
    nameText:Phaser.GameObjects.Text
    isDead = false
    mhp:integer = 0
    _hp:integer = 0
    set hp(val:integer)
    {
        this.setHP(val,true)
    }
    get hp()
    {
        let st = this.getModifiedStatus()
        return Math.min(this._hp,st.mhp)
    }
    mmp:integer = 0
    _mp:integer = 0
    set mp(val:integer)
    {
        val = Math.floor(val)
        let st = this.getModifiedStatus()
        let new_mp = Math.min(val,st.mmp)
        let numberType = val - this.mp < 0 ? NumberType.MPDAMAGE : NumberType.MPHEAL
        let delta = new_mp - this._mp
        this._mp = new_mp
        if (this.container)
        {
            new NumberUI(this.scene,this.x,this.y-32,Math.abs(delta),numberType)
        }
        this._mp = Math.max(this._mp,0)
    }
    get mp()
    {
        let st = this.getModifiedStatus()
        return Math.min(this._mp,st.mmp)
    }
    hpBar: Phaser.GameObjects.Rectangle
    hpBarBackgroud: Phaser.GameObjects.Rectangle
    mpBar: Phaser.GameObjects.Rectangle
    atk:integer = 0
    def:integer = 0
    spd:integer = 0
    mag:integer = 0
    mdef:integer = 0

    sprites: Phaser.GameObjects.Sprite[] = []
    spriteIndeces: number[] = []
    spriteTints: number[] = []

    moveAI: MoveAI
    actionTargetAI: ActionTargetAI
    actionAI: ActionAI
    actions : Action[] = []
    actionInterval:number = 0

    weapon : Weapon
    weaponSprite: Phaser.GameObjects.Sprite

    originalTeam:Team = 0
    currentTeam:Team = 0
    teamCircle:Phaser.GameObjects.Ellipse

    buffs:{[key:string]:Buff} = {}
    buffIcons:{[key:string]:Phaser.GameObjects.Sprite} = {}

    statusRecalcFlag = true
    calcedModifiedStatus: StatusObject|null = null

    traits:Passive[] = []

    set x(val:number)
    {
        this.container.x = val
    }
    get x()
    {
        return this.container.x
    }
    set y(val:number)
    {
        this.container.y = val
    }
    get y()
    {
        return this.container.y
    }
    get velocity():Phaser.Math.Vector2 | MatterJS.Vector
    {
        if (!this.container.body) return Phaser.Math.Vector2.ZERO
        return this.container.body.velocity
    }
    get position():Phaser.Math.Vector2 | MatterJS.Vector
    {
        if (!this.container.body) return Phaser.Math.Vector2.ZERO
        return this.container.body.position
    }

    constructor(scene:AbstractBattleScene,name:string,team?:Team)
    {
        this.scene = scene
        this.moveAI = new MoveToNearestEnemy(this,scene)
        this.actionTargetAI = new ActionToNearest(this,scene)
        this.actionAI = new RandomActionAI(this,scene,new MersenneTwister())
        this.weapon = new Weapon(0,0)
        this.setStatus(name,scene)

        this.container = scene.add.container(0,0)
        scene.physics.world.enable(this.container)
        if (this.container.body instanceof Phaser.Physics.Arcade.Body)
        {
            this.container.body.setSize(16,32)
            this.container.body.setOffset(8,-16)
        }
        
        this.sprites[AvatarType.HAIR] = scene.add.sprite(0,-12,"avatars",getAvatarSpriteFrame(AvatarType.HAIR,this.spriteIndeces[AvatarType.HAIR]))
        this.sprites[AvatarType.HEAD] = scene.add.sprite(0,-12,"avatars",getAvatarSpriteFrame(AvatarType.HEAD,this.spriteIndeces[AvatarType.HEAD]))
        this.sprites[AvatarType.TOPS] = scene.add.sprite(0,0,"avatars",getAvatarSpriteFrame(AvatarType.TOPS,this.spriteIndeces[AvatarType.TOPS]))
        this.sprites[AvatarType.BOTTOMS] = scene.add.sprite(0,12,"avatars",getAvatarSpriteFrame(AvatarType.BOTTOMS,this.spriteIndeces[AvatarType.BOTTOMS]))
        this.sprites[AvatarType.BOOTS] = scene.add.sprite(0,24,"avatars",getAvatarSpriteFrame(AvatarType.BOOTS,this.spriteIndeces[AvatarType.BOOTS]))

        for (let i of [AvatarType.BOTTOMS,AvatarType.TOPS,AvatarType.BOOTS,AvatarType.HEAD,AvatarType.HAIR])
        {
            let o = this.sprites[i]
            o.tint = this.spriteTints[i]
            o.setScale(2)
            this.container.add(o)
        }

        {
            let o = scene.add.sprite(8,-4,"weapons",getWeaponSpriteFrame(this.weapon.weaponType,this.weapon.index))
            o.setScale(2)
            this.container.add(o)
            this.weaponSprite = o
        }

        {
            let o = scene.add.text(this.x,this.y-36,this.name,{font:"bold 8px sans-serif",color:"#ffffff"})
            // o.setScale(1/4,1/4)
            o.setOrigin(1/2,1/2)
            o.setDepth(20000)
            o.setResolution(2)
            o.setAlpha(1/2)
            this.nameText = o
        }

        {
            let o = scene.add.rectangle(this.x-16,this.y-24,32,2,0x00ff00)
            o.setOrigin(0,1/2)
            o.setDepth(20000)
            o.setAlpha(1/2)
            this.hpBar = o
        }

        {
            let o = scene.add.rectangle(this.x-16,this.y-24,32,2,0xff0000)
            o.setOrigin(0,1/2)
            o.setDepth(20000-1)
            o.setAlpha(1/2)
            this.hpBarBackgroud = o
        }

        {
            let o = scene.add.rectangle(this.x-16,this.y-24,32,2,0x0000ff)
            o.setOrigin(0,1/2)
            o.setDepth(20000)
            o.setAlpha(1/2)
            this.mpBar = o
        }

        {
            let col = 0x000000
            if (team == Team.ALLY) col = 0x0000ff
            else if (team == Team.ENEMY) col = 0xff0000
            let o = scene.add.ellipse(this.x,this.y,32,16,col)
            o.setAlpha(1/2)
            o.setPosition(this.x,this.y+24)
            this.teamCircle = o
        }
        

        if (team)
        {
            this.originalTeam = team
            this.currentTeam = team

            if (team == Team.ALLY) scene.allies.push(this)
            else if (team == Team.ENEMY) scene.enemies.push(this)
        }
        scene.setLayer(this)



        this.statusRecalcFlag = true
    }

    update(battleField:IBattleField)
    {
        if (this.isDead) return
        if (!this.container.body) return
        let st = this.getModifiedStatus()
        if(!st.moveAI || !st.actionAI) return

        this.x = Math.max(this.x,32)
        this.x = Math.min(this.x,480-32)
        this.y = Math.max(this.y,32)
        this.y = Math.min(this.y,720-128-32)

        if (this.velocity.x == 0 && this.velocity.y == 0)
        {
            this.setAnimIdol()
        }
        else
        {
            this.setAnimWalk()
        }
        st.moveAI.move()
        if (this.velocity.x < 0)
        {
            this.setFlipX(true)
        }
        else if (this.velocity.x > 0)
        {
            this.setFlipX(false)
        }
        this.container.setDepth(this.y)

        if (this.actionInterval <= 0)
        {
            this.weaponSprite.setVisible(true)
            let action = st.actionAI.choose()
            if (action)
            {
                action.actionTemplate()
            }
            this.actionInterval = 30000
        }
        else
        {
            this.actionInterval -= Math.max(st.spd,30)
        }


        // バフのupdate
        for(let key in this.buffs)
        {
            let o = this.buffs[key]
            o.update()
        }

        // UI表記の無い自然治癒
        
        if(Math.random() <= st.mmp/2000)
        {
            this._mp = Math.min(this.mp+1,st.mmp)
        }

        this.teamCircle.setPosition(this.x,this.y+24)

        this.nameText.setPosition(this.x,this.y-36)
        this.hpBar.setPosition(this.x-16,this.y-28)
        this.hpBar.setScale(this.hp/st.mhp,1)
        this.hpBarBackgroud.setPosition(this.x-16,this.y-28)
        this.mpBar.setPosition(this.x-16,this.y-24)
        this.mpBar.setScale(this.mp/st.mmp,1)

        let buffCount = 0
        for (let key in this.buffIcons)
        {
            let o = this.buffIcons[key]
            let dx = (buffCount%6) * 8 - 24
            let dy = Math.floor(buffCount/6) * (-8) - 42
            o.setPosition(this.x+dx,this.y+dy)
            buffCount++
        }
    }

    setHP(val:number,isShowHPDmg:boolean)
    {
        if(this.isDead)return
        val = Math.floor(val)
        let st = this.getModifiedStatus()
        let new_hp = Math.min(val,st.mhp)
        let numberType = val - this.hp < 0 ? NumberType.DAMAGE : NumberType.HPHEAL
        let delta = new_hp - this._hp
        this._hp = new_hp
        if (this.container &&  isShowHPDmg)
        {
            new NumberUI(this.scene,this.x,this.y-32,Math.abs(delta),numberType)
        }
        if (this._hp <= 0)
        {
            this._hp = Math.max(this._hp,0)
            this.die()
        }
    }

    changeMPWithoutShown(delta:number)
    {
        let st = this.getModifiedStatus()
        this._mp = Math.max(0,Math.min(this.mp+delta,st.mmp))
    }
    
    // MPを使用する際は、-MPとしないように。ダメージ表記が出てしまう。
    consumeMP(consumeMP:integer)
    {
        if (consumeMP < 0) console.error("Character.tryUseMPに負のMPが入れられました。")
        if (this.mp >= consumeMP)
        {
            this._mp -= consumeMP
            return true
        }
        return false
    }

    setAnimIdol()
    {
        this.sprites[AvatarType.BOTTOMS].stop()
        this.sprites[AvatarType.BOOTS].stop()
        this.sprites[AvatarType.BOTTOMS].setFrame(getAvatarSpriteFrame(AvatarType.BOTTOMS,this.spriteIndeces[AvatarType.BOTTOMS]))
        this.sprites[AvatarType.BOOTS].setFrame(getAvatarSpriteFrame(AvatarType.BOOTS,this.spriteIndeces[AvatarType.BOOTS]))
    }
    setAnimWalk()
    {
        let st = this.getModifiedStatus()
        this.sprites[AvatarType.BOTTOMS].anims.play(`bottoms_walk_${this.spriteIndeces[AvatarType.BOTTOMS]}`, true)
        this.sprites[AvatarType.BOOTS].anims.play(`boots_walk_${this.spriteIndeces[AvatarType.BOOTS]}`, true)
        this.sprites[AvatarType.BOTTOMS].anims.msPerFrame = 1000*100/st.spd
        this.sprites[AvatarType.BOOTS].anims.msPerFrame = 1000*100/ st.spd
    }

    setVelocity(x:number,y:number)
    {
        if (this.container.body instanceof Phaser.Physics.Arcade.Body)
        {
            this.container.body.setVelocity(x,y)
        }
    }

    //ここが移動距離の定義
    getMoveRange()
    {
        let st = this.getModifiedStatus()
        return st.spd/10
    }

    setPosition(x:number,y:number)
    {
        this.x = x
        this.y = y
        this.teamCircle.setPosition(this.x,this.y+24)
    }

    getAtkRange()
    {
        let maxRange = 0
        if (this.getModifiedStatus().actionAI instanceof OnlyWeaponAttackActionAI)
        {
            return this.actions[0].range
        }

        for (let o of this.actions)
        {
            if ([TargetType.ENEMY,TargetType.ENEMYALL,TargetType.ALL].includes(o.type) && o.isMeetCondition())
            {
                maxRange = Math.max(maxRange,o.range)
            }
        }
        return maxRange
    }

    // これは勘違いしやすそうだが、charaのflipに応じて、xを-1倍して返す関数。
    getFlipX(x:number)
    {
        if (this.container.scaleX > 0)
        {
            return x
        }
        else
        {
            return -x
        }
    }

    setFlipX(isFlip:boolean)
    {
        if (isFlip)
        {
            this.container.setScale(-1,1)
            if (this.container.body instanceof Phaser.Physics.Arcade.Body)
            {
                this.container.body.setOffset(8,-16)
            }
        }
        else
        {
            this.container.setScale(1,1)
            if (this.container.body instanceof Phaser.Physics.Arcade.Body)
            {
                this.container.body.setOffset(-8,-16)
            }
        }
    }

    setVelocityPointTo(x:number,y:number)
    {
        let st = this.getModifiedStatus()
        let vec = new Phaser.Math.Vector2(x-this.x,y-this.y).normalize().scale(this.getMoveRange())
        this.setVelocity(vec.x,vec.y)
    }

    getModifiedStatus():StatusObject
    {
        if(!this.statusRecalcFlag && this.calcedModifiedStatus != null)
        {
            return this.calcedModifiedStatus
        }

        let o:StatusObject = {
            mhp:this.mhp,
            mmp:this.mmp,
            atk:this.atk,
            def:this.def,
            spd:this.spd,
            mag:this.mag,
            mdef:this.mdef,
            moveAI:this.moveAI,
            actionAI:this.actionAI,
            actionTargetAI:this.actionTargetAI,
        } as StatusObject
        
        let weapon = this.weapon
        o.atk += weapon.atk
        o.mag += weapon.mag
        o.def += weapon.def
        o.spd += weapon.spd
        o.mhp += weapon.hp
        o.mmp += weapon.mp
        o.mdef += weapon.mdef

        for (let key in this.buffs)
        {
            let buff = this.buffs[key]
            if(!buff) continue
            o = buff.statusModify(o)
        }
        o.mhp = Math.max(1, o.mhp)
        o.mmp = Math.max(0, o.mmp)
        o.atk = Math.max(0, o.atk)
        o.def = Math.max(0, o.def)
        o.spd = Math.max(0, o.spd)
        o.mag = Math.max(0, o.mag)
        o.mdef = Math.max(0,o.mdef)

        this.statusRecalcFlag = false
        this.calcedModifiedStatus = o
        return o
    }

    addBuff(o:Buff)
    {
        o.uuid = Phaser.Utils.String.UUID()
        this.statusRecalcFlag = true
        this.buffs[o.uuid] = o

        if(o.buffIconIndex >= 0)
        {
            let icon = this.scene.add.sprite(this.x,this.y,"sprites",o.buffIconIndex)
            icon.setScale(2)
            this.buffIcons[o.uuid] = icon
        }
    }

    removeBuff(o:Buff)
    {
        if (!Object.keys(this.buffs).includes(o.uuid))
        {
            console.warn("Character.removeBuff:指定したバフがbuffsに無い。")
            if(DEBUG_MODE)return
        }
        this.statusRecalcFlag = true
        let uuid = o.uuid
        delete this.buffs[o.uuid]

        if(o.buffIconIndex >= 0)
        {
            let icon = this.buffIcons[uuid]
            icon.destroy()
            delete this.buffIcons[uuid]
        }
    }

    die()
    {
        this.isDead = true
        if (this.container.scaleX < 0)
        {
            this.container.setRotation(Math.PI/2)
        }
        else
        {
            this.container.setRotation(-Math.PI/2)
        }
        this.setAnimIdol()
        this.setVelocity(0,0)
        if (this.container.body instanceof Phaser.Physics.Arcade.Body)
        {
            this.container.body.enable = false
        }
        for(let key in this.buffs)
        {
            let buff = this.buffs[key]
            buff.destroy()
        }
        this.container.setAlpha(1/4)
        this.hpBar.setAlpha(this.hpBar.alpha/4)
        this.mpBar.setAlpha(this.mpBar.alpha/4)
        this.nameText.setAlpha(this.nameText.alpha/4)
        this.teamCircle.setAlpha(0)
    }

    setStatus(name:string,scene:AbstractBattleScene)
    {
        let mt = new MersenneTwister()
        let unicodes = []
        for (let i = 0; i < name.length; i++)
        {
            unicodes.push(name.charCodeAt(i))
        }
        var nums = []
        for (let i = 0; i < unicodes.length; i++)
        {
            mt.seed(unicodes[i])
            nums.push(mt.int())
        }

        // ここで、完全な名前による乱数が完成する。
        mt.seedArray(unicodes)

        this.name = name
        
        this.mhp = Math.max(1,mt.int()%800+mt.int()%800+mt.int()%800+mt.int()%800+mt.int()%800)
        this.hp = this.mhp
        this.mmp = Math.max(1,mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200)
        this.statusRecalcFlag = true
        this.mp = this.mmp
        this.atk = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200
        this.def = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200
        this.spd = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200
        this.mag = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200
        this.mdef = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200

        for (let i = 0; i < 5; i++)
        {
            if (i == AvatarType.HEAD)
            {
                var cols = [0xfee2d2,0xfdcbb0,0xe6904e,0xcd683d,0x9e4539]
                this.spriteTints.push(cols[mt.int()%cols.length])
                continue
            }
            this.spriteTints.push(mt.int()%(0x1000000))
        }
        this.spriteIndeces.push(mt.int()%15)
        this.spriteIndeces.push(mt.int()%6)
        this.spriteIndeces.push(mt.int()%6)
        this.spriteIndeces.push(mt.int()%2)
        this.spriteIndeces.push(mt.int()%1)

        switch (mt.int()%3)
        {
            case 0: this.moveAI = new MoveToNearestEnemy(this,scene); break;
            case 1: this.moveAI = new MoveToLowestHPEnemy(this,scene); break;
            case 2: this.moveAI = new MoveToRangeEdge(this,scene); break;
        }
        switch (mt.int()%3)
        {
            case 0: this.actionAI = new RandomActionAI(this,scene,mt); break;
            case 1: this.actionAI = new WeightActionAI(this,scene,mt); break;
            case 2: this.actionAI = new GoalSettingActionAI(this,scene,mt); break;
        }

        let weaponType = mt.int()%4 as WeaponType
        let weaponIndex = 0
        if (weaponType == WeaponType.SWORD) weaponIndex = mt.int()%6
        else if (weaponType == WeaponType.BOW) weaponIndex = mt.int()%6
        else if (weaponType == WeaponType.STAFF) weaponIndex = mt.int()%5
        else if (weaponType == WeaponType.GUN) weaponIndex = mt.int()%3
        this.weapon = new Weapon(weaponType,weaponIndex)

        this.actions.push(new AttackWithWeapon(this,scene,this.weapon))

        while (this.actions.length < 4)
        {
            let o = null
            switch(mt.int()%20)
            {
                case 0: o = new Heal(this,scene,mt); break;
                case 1: o = new FireBall(this,scene,mt); break;
                case 2: o = new IcicleRain(this,scene,mt); break;
                case 3: o = new SummonThunderClouds(this,scene,mt); break;
                case 4: o = new CallAllies(this,scene,mt); break;
                case 5: o = new Meditation(this,scene,mt); break;
                case 6: o = new Beam(this,scene,mt); break;
                case 7: o = new StatusBoost(this,scene,mt); break;
                case 8: o = new StatusDown(this,scene,mt); break;
                case 9: o = new SpiritBless(this,scene,mt); break;
                case 10: o = new BubbleBreath(this,scene,mt); break;
                case 11: o = new Reflect(this,scene,mt); break;
                case 12: o = new LifeDrain(this,scene,mt); break;
                case 13: o = new ManaDrain(this,scene,mt); break;
                case 14: o = new Dispel(this,scene,mt); break;
                case 15: o = new Cover(this,scene,mt); break;
                case 16: o = new Clearance(this,scene,mt); break;
                case 17: o = new VenomShot(this,scene,mt); break;
                case 18: o = new SkillLock(this,scene,mt); break;
                case 19: o = new Regenerate(this,scene,mt); break;
            }
            if(!o) continue
            this.actions.push(o)
        }

        while (true)
        {
            let o = null
            switch(mt.int()%2)
            {

            }
            if(!o) break
            this.traits.push(o)
        }
        

        // console.log(this)

        
        

    }
    
    destroy()
    {
        this.teamCircle.destroy()
        this.container.destroy(true)
    }

    static createAnims(scene:Phaser.Scene)
    {
        for(let i = 0; i < 2; i++)
        {
            scene.anims.create({
                key: `bottoms_walk_${i}`,
                frames: scene.anims.generateFrameNumbers("avatars", { start: getAvatarSpriteFrame(AvatarType.BOTTOMS,i), end: getAvatarSpriteFrame(AvatarType.BOTTOMS,i)+3 }),
                frameRate: 5,
                repeat: -1,
            })
        }
        for(let i = 0; i < 1; i++)
        {
            scene.anims.create({
                key: `boots_walk_${i}`,
                frames: scene.anims.generateFrameNumbers("avatars", { start: getAvatarSpriteFrame(AvatarType.BOOTS,i), end: getAvatarSpriteFrame(AvatarType.BOOTS,i)+3 }),
                frameRate: 5,
                repeat: -1,
            })
        }
    }
}



class BattlePage extends Phaser.Scene
{
    static allyArea: HTMLTextAreaElement
    static enemyArea: HTMLTextAreaElement

    constructor()
    {
        super({key:getSceneName(PageIndex.BATTLE)})
    }

    create()
    {
        // this.add.text(32,32,"味方",{font:"32px sans-serif",color:"#000000"})
        // this.add.text(32,80+192+16,"敵",{font:"32px sans-serif",color:"#000000"})

    }

    static createHTML()
    {
        let page = HTML.getPageElement(PageIndex.BATTLE)
        BattlePage.allyArea = HTML.createHTMLElement("textarea",240,32+96,432,192,page) as HTMLTextAreaElement
        BattlePage.allyArea.placeholder = "青チームの名前１\n青チームの名前２\n青チームの名前３\n青チームの名前４"
        BattlePage.enemyArea = HTML.createHTMLElement("textarea",240,32+192+32+96,432,192,page) as HTMLTextAreaElement
        BattlePage.enemyArea.placeholder = "赤チームの名前１\n赤チームの名前２\n赤チームの名前３\n赤チームの名前４"

        let b1 = HTML.createHTMLElement("button",360,32+192+32+192+48,160,64,page) as HTMLButtonElement
        b1.innerText = "戦闘開始"
        b1.onclick = BattlePage.startBattle
    }

    static startBattle()
    {
        HTML.gotoScene("battle")
        BattleScene.allyNames = BattlePage.allyArea.value.split("\n")
        BattleScene.enemyNames = BattlePage.enemyArea.value.split("\n")
        BattleScene.battleMode = BattleMode.BLUERED
    }

    update()
    {

    }
}

class ChallengePage extends Phaser.Scene
{
    static challengeCompleteList:boolean[] = []
    static challengeCount:integer = 20

    constructor()
    {
        super({key:getSceneName(PageIndex.CHALLENGE)})
        ChallengePage.initChallengeData()
    }

    static createHTML()
    {
        let page = HTML.getPageElement(PageIndex.CHALLENGE)
        let div = HTML.createHTMLElement("div",240,16,480,720-128-32,page) as HTMLDivElement
        div.classList.add("scrollBox")
        for (let i = 1; i <= ChallengePage.challengeCount; i++)
        {
            let o = HTML.createHTMLElement("button",240,32+48*(i-1),480-32,48,div) as HTMLButtonElement
            // o.innerText = ChallengePage.getChallengeName(i)
            o.innerText = "LOCKED"
            o.classList.add("challenge")
            o.classList.add("lock")
            o.onclick = () => {
                if(!ChallengePage.isChallengeAble(i))return
                ChallengeBattlePage.level = i
                HTML.selectPage(PageIndex.CHALLENGEBATTLE)
            }
        }
    }

    static getChallengeName(level:integer)
    {
        let o = `Lv${level}: `
        switch (level)
        {
            case LevelNum.ARMY4: o += "兵士×4"; break;
            case LevelNum.ARMY3: o += "兵士×4(3人制限)"; break;
            case LevelNum.KING: o += "王と護衛"; break;
            case LevelNum.CARAVAN: o += "キャラバン隊"; break;
            case LevelNum.RYUGU_CASTLE: o += "竜宮城"; break;
            case LevelNum.NO_VIOLENCE: o += "微暴力・非服従"; break;
            case LevelNum.ADVANCED_ARENA: o += "上級アリーナ(ランダム)"; break;
            case LevelNum.SUPREME_ARENA: o += "超級アリーナ(ランダム)"; break;
            case LevelNum.GOD_ARENA: o += "神級アリーナ(ランダム)"; break;
            case LevelNum.DARK_ORDER: o += "暗黒騎士団"; break;
            case LevelNum.DEMONSTORATORS: o += "デモ隊鎮圧"; break;
            case LevelNum.SHURA: o += "修羅の国"; break;
            case LevelNum.LASTBATTLE: o += "ラストバトル！"; break;
            case LevelNum.SUBJUGATE_SQUAD: o += "竜人討伐隊入隊"; break;
            case LevelNum.DEMON_WORLD: o += "魔界"; break;
            case LevelNum.SPIRIT_GOD: o += "大精霊降臨"; break;
            case LevelNum.BLACK_HOLE: o += "ブラックホール"; break;
            case LevelNum.MAGIC_WORLD: o += "物理禁止ワールド"; break;
            case LevelNum.WEAK_PARTY: o += "ルーキーズ"; break;
            case LevelNum.THEFTS: o += "盗賊取締"; break;
        }

        return o
    }

    static initChallengeData()
    {
        let o:boolean[] = []
        for(let i = 0; i < ChallengePage.challengeCount; i++)
        {
            o.push(false)
        }
        ChallengePage.challengeCompleteList = o
    }

    static challengeComplete(level:integer)
    {
        let o = ChallengePage.getChallengeButton(level)
        if(!o)return
        ChallengePage.challengeCompleteList[level-1] = true
        o.classList.add("complete")
        ChallengePage.setChallengeLock()
    }

    static getChallengeButton(level:integer): HTMLButtonElement|null
    {
        let button = HTML.getPageElement(PageIndex.CHALLENGE).getElementsByClassName("scrollBox")[0].children[level-1] as HTMLButtonElement
        if(!button)
        {
            console.log("ChallengePage.getChallengeButtonでbuttonが取得できませんでした。")
            console.log(`button:${button}, level:${level}`)
            return null
        }
        return HTML.getPageElement(PageIndex.CHALLENGE).getElementsByClassName("scrollBox")[0].children[level-1] as HTMLButtonElement
    }

    static isChallengeAble(level:integer)
    {
        let completeNum = ChallengePage.challengeCompleteList.filter(o=>o==true).length
        return level <= completeNum+2
    }

    static setChallengeLock()
    {
        let completeNum = ChallengePage.challengeCompleteList.filter(o=>o==true).length
        let unlockNum = Math.min(completeNum+2,ChallengePage.challengeCount)
        for(let level = 1; level <= unlockNum; level++)
        {
            let o = ChallengePage.getChallengeButton(level)
            if(!o)continue
            if(o.classList.contains("lock"))
            {
                o.classList.remove("lock")
                o.innerText = ChallengePage.getChallengeName(level)
            }
        }
    }
}

enum LevelNum
{
    WEAK_PARTY = 1,
    ARMY4,
    ARMY3,
    THEFTS,
    KING,
    CARAVAN,
    RYUGU_CASTLE,
    DEMONSTORATORS,
    NO_VIOLENCE,
    ADVANCED_ARENA,
    SHURA,
    SUPREME_ARENA,
    LASTBATTLE,
    SUBJUGATE_SQUAD,
    DEMON_WORLD,
    DARK_ORDER,
    SPIRIT_GOD,
    BLACK_HOLE,
    MAGIC_WORLD,
    GOD_ARENA,
}

class ChallengeBattlePage extends Phaser.Scene
{
    static allyArea:HTMLTextAreaElement
    static level:integer = 0
    static enemies:string[] = []
    static rule:()=>void
    static startRule:()=>void
    static updateRule:()=>void
    static allyNumLimit:integer = 0

    constructor()
    {
        super({key:getSceneName(PageIndex.CHALLENGEBATTLE)})
    }

    create()
    {
        ChallengeBattlePage.setLevel()
        let o = this.add.text(32,32+48+192+32,`（今回の対戦可能人数は${ChallengeBattlePage.allyNumLimit}人まで）`,{font:"24px sans-serif",color:"#000000"})
        o.setOrigin(0,1/2)
        
    }

    static createHTML()
    {
        let page = HTML.getPageElement(PageIndex.CHALLENGEBATTLE)
        ChallengeBattlePage.allyArea = HTML.createHTMLElement("textarea",240,32+96+48,432,192,page) as HTMLTextAreaElement
        ChallengeBattlePage.allyArea.placeholder = "味方の名前１\n味方の名前２\n味方の名前３\n味方の名前４"

        {
            let o = HTML.createHTMLElement("button",360,32+192+32+192+48,160,64,page) as HTMLButtonElement
            o.innerText = "戦闘開始"
            o.onclick = ChallengeBattlePage.startBattle
        }
        {
            let o = HTML.createHTMLElement("button",80,32,160,64,page) as HTMLButtonElement
            o.innerText = "戻る"
            o.onclick = () => {
                HTML.selectPage(PageIndex.CHALLENGE)
            }
        }
    }

    static startBattle()
    {
        HTML.gotoScene("battle")
        ChallengeBattlePage.setLevel()
        BattleScene.allyNames = ChallengeBattlePage.getCharacters()
        BattleScene.enemyNames = ChallengeBattlePage.enemies.concat()
        BattleScene.battleMode = BattleMode.CHALLENGE

        ChallengeBattlePage.rule()
    }

    static getCharacters()
    {
        let o = ChallengeBattlePage.allyArea.value.split("\n")
        return o.slice(0,ChallengeBattlePage.allyNumLimit)
    }

    static setLevel()
    {
        let n:integer = 0
        let o:string[] = []
        ChallengeBattlePage.rule = () => {}
        ChallengeBattlePage.updateRule = () => {}
        switch(ChallengeBattlePage.level)
        {
            case LevelNum.ARMY4: n = 4; o = ["兵士","兵士","兵士","兵士"]; break;
            case LevelNum.ARMY3: n = 3; o = ["兵士","兵士","兵士","兵士"]; break;
            case LevelNum.WEAK_PARTY: n = 4; o = ["剣士アルファ","魔導士イータ","弓使いガンマ","銃士シータ"]; break;
            case LevelNum.THEFTS: n = 4; o = ["盗賊ダヴォン","盗賊ジョバニ","盗賊チャズ","盗賊ナディア","盗賊パトリック"]; break;
            case LevelNum.KING: n = 4; o = ["護衛隊U","護衛隊U","護衛隊","護衛隊","キング王"]; break;
            case LevelNum.CARAVAN: n = 4; o = ["傭兵テリー","傭兵ジョーダン","傭兵キラ","傭兵カリー","商人アミーヤ"]; break;
            case LevelNum.RYUGU_CASTLE: n = 4; o = ["乙姫護衛隊193","乙姫護衛隊193","乙姫護衛隊193","乙姫護衛隊193","乙姫護衛隊193","🌊乙姫"]; break;
            case LevelNum.ADVANCED_ARENA: n = 4; o = ChallengeBattlePage.getRandom([
                "ペニー","チャップリン","グレン","マンイーター","イーター","マイク","スーパー","ネイチャ","デタ","カット","バイバイン","シールズ皇太子","バンドル","まったり","プリンス","グー","パイ","ジョ","ウーバ","テン","プラシーボ","A","用心棒","リウム","サモ","バロン","ガーゴイル","天真爛漫","メイジE"
            ],4); break;
            case LevelNum.NO_VIOLENCE: n = 1; o = ["ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー","ガンジー"]; break;
            case LevelNum.SUPREME_ARENA: n = 4; o = ChallengeBattlePage.getRandom([
                "ファクト","アーサー","100","絵本","パック","パンプキン","グレープス","プレイアブル","ランカー","ミカエルロッド","神宮","No.59","No.69"
            ],4); break;
            case LevelNum.GOD_ARENA: n = 4; o = ChallengeBattlePage.getRandom([
                "海賊アレクシア","アーサー","メイジドロシー","ヒーラーカルロス","神官戦士セス","乞食ザッケリー"
            ],4); break;
            case LevelNum.DARK_ORDER: n = 4; o = ["暗黒騎士団ベニー","暗黒騎士団エリッカ","暗黒騎士団コールマン","暗黒騎士団エリヤ","暗黒騎士団長モシェ"]; break;
            case LevelNum.DEMONSTORATORS: n = 6; o = ["暴徒ブロンソン","デモ参加者108","デモ隊118","デモ隊124","暴徒化したメラニー","暴徒と化したジリアン","暴徒と化したリリー"]; break;
            case LevelNum.SHURA: n = 6; o = ["泥酔するリディア","万引き犯キャシー","瞑想するカテリン","道端で寝るケント","犬の真似をするアルヴィン","エアガンを撃つメレディ","たこ焼きを投げるデオンテ","殺人鬼エマーソン","殺人鬼カザンドラ"]; break;
            case LevelNum.LASTBATTLE: n = 1; o = ["大魔王アシュティン","側近アンドレアス","側近アルレーン"]; ChallengeBattlePage.rule = () => {
                BattleScene.allyNames.push("勇者ロジェリオ")
            }; break;
            case LevelNum.SUBJUGATE_SQUAD: n = 1; o = ["竜人イーストン","竜人アルレーン","竜人キーオン","竜人カリッサ","竜人ケイト"];
            ChallengeBattlePage.rule = () => {
                BattleScene.allyNames = BattleScene.allyNames.concat([
                    "レイラ","リロイ","チャド","スカイラ","ジョーイ","キャサリン","ジャダ","エフレイン"
                ])
            }; break;
            case LevelNum.DEMON_WORLD: n = 4; o = ChallengeBattlePage.getRandom([
                "悪魔アルデン","悪魔デオンテ","悪魔ナタリー","邪悪なランディ","邪悪なマルセル","邪悪なマリツァ","邪悪なリカルド","悪魔神官マレーネ","悪魔神官レイチェル","魔人モハメド","術師ディアナ"
            ],4);
            ChallengeBattlePage.updateRule = () => {
                for(let o of BattleScene.getAllLivingBattlers())
                {
                    o.changeMPWithoutShown(10)
                }
            }; break;
            case LevelNum.SPIRIT_GOD: n = 4; o = ["大精霊レイセ"];
            ChallengeBattlePage.updateRule = () => {
                for(let o of BattleScene.scene.enemies)
                {
                    if(Math.random() >= 0.5) new SpiritBuff(o,AbstractBattleScene.scene)
                }
            }; break;
            case LevelNum.BLACK_HOLE: n = 4; o = ChallengeBattlePage.getRandom([
                "火星人ルーカス","火星人ヘリバート","金星人マクシミリアン","金星人ヘイリー","異星人エルマー","異星人ダニエラ"
            ],4);
            ChallengeBattlePage.updateRule = () => {
                for(let o of BattleScene.scene.enemies)
                {
                    if(Math.random() >= 0.99) new StatusAddBuff(o,BattleScene.scene,Number.MAX_VALUE,StatusType.SPD,100)
                }
            }; break;
            case LevelNum.MAGIC_WORLD: n = 4; o = ChallengeBattlePage.getRandom([
                "上級魔導士ダリル","上級魔導士チャーリー","大魔導士プリンス","大魔導士ナンシー","大魔導士ドミニク","大魔導士カッサンドラ","大魔導士アントワーヌ",
            ],4)
            ChallengeBattlePage.updateRule = () => {
                for(let o of BattleScene.getAllBattlers())
                {
                    if(o.def!=1000000)
                    {
                        o.statusRecalcFlag = true
                        o.def = 1000000
                    }
                }
            }; break;
            
        }
        ChallengeBattlePage.enemies = o
        ChallengeBattlePage.allyNumLimit = n
    }



    static getRandom(arr:string[],num:integer)
    {
        let o = []
        for(let i = 0; i < 4; i++)
        {
            o.push(Phaser.Utils.Array.GetRandom(arr))
        }
        return o
    }
}

enum BattleResult
{
    NONE,
    BLUEWIN,
    REDWIN,
    DRAW,
}

enum BattleMode
{
    BLUERED,
    CHALLENGE,
    RANKBATTLE,
}

class BattleScene extends AbstractBattleScene
{
    static allyNames:string[] = []
    static enemyNames:string[] = []
    static battleMode:BattleMode
    BattleEndFlag = false

    // 実際に戦闘するシーン
    constructor()
    {
        super({key:"battle"})
    }

    create()
    {
        super.create()
        this.BattleEndFlag = false
        Character.createAnims(this)
        this.allies = []
        this.enemies = []

        for (let i = 0; i < BattleScene.allyNames.length; i++)
        {
            let o = new Character(this, BattleScene.allyNames[i],Team.ALLY)
            o.x = 100 + 32*(Math.floor(i/8))
            o.y = 100 + 64*(i%8)
            // this.allies.push(o)
        }
        for (let i = 0; i < BattleScene.enemyNames.length; i++)
        {
            let o = new Character(this, BattleScene.enemyNames[i],Team.ENEMY)
            o.x = 380 - 32*(Math.floor(i/8))
            o.y = 100 + 64*(i%8)
            // this.enemies.push(o)
        }
    }

    update()
    {
        super.update()

        for (let layer of this.layers)
        {
            for (let _o of layer.getChildren())
            {
                let o = AbstractBattleScene.objects[_o.name]
                if (o instanceof Character)
                {
                    o.update(this)
                }
                else if (o instanceof Bullet)
                {
                    o.update()
                }
            }
        }
        for (let key in this.effects2)
        {
            let o = this.effects2[key]
            o.update()
        }

        if(this.isBattleMode(BattleMode.CHALLENGE))
        {
            ChallengeBattlePage.updateRule()
        }

        if(!this.BattleEndFlag && this.isBattleEnd())
        {
            this.battleEnd()
        }
    }

    isBattleEnd()
    {
        let teamAlive = (team:Character[]) => {
            for (let o of team)
            {
                if (o.hp > 0) return true
            }
            return false
        }
        return teamAlive(this.allies) && teamAlive(this.enemies) ? false : true
    }

    getBattleResult():BattleResult
    {
        let allyAlive = false
        let enemyAlive = false

        for (let o of this.allies)
        {
            if (o.hp > 0) allyAlive = true 
        }
        for (let o of this.enemies)
        {
            if (o.hp > 0) enemyAlive = true 
        }

        if(allyAlive && !enemyAlive) return BattleResult.BLUEWIN
        else if(!allyAlive && enemyAlive) return BattleResult.REDWIN
        else if(!allyAlive && !enemyAlive) return BattleResult.DRAW
        
        return BattleResult.NONE
    }

    getLeftCount():{allyNum:integer,enemyNum:integer}
    {
        let allyNum = 0
        let enemyNum = 0
        for (let o of this.allies)
        {
            if (o.hp > 0) allyNum += 1
        }
        for (let o of this.enemies)
        {
            if (o.hp > 0) enemyNum += 1
        }
        return {allyNum:allyNum,enemyNum:enemyNum}
    }

    battleEnd()
    {
        this.BattleEndFlag = true
        let frame = 0
        let result = this.getBattleResult()
        if(this.isBattleMode(BattleMode.CHALLENGE))
        {
            frame = result == BattleResult.BLUEWIN ? frame = 2 : frame = 3
        }
        else if(this.isBattleMode(BattleMode.BLUERED))
        {
            if(result == BattleResult.DRAW) frame = 4
            else frame = result == BattleResult.BLUEWIN ? frame = 0 : frame = 1
        }
        else if(this.isBattleMode(BattleMode.RANKBATTLE))
        {
            if(result == BattleResult.DRAW) frame = 4
            else frame = result == BattleResult.BLUEWIN ? frame = 5 : frame = 6
        }

        {
            let o = this.add.sprite(240,(720-128)/2,"texts",frame)
            o.setScale(4)
            o.setDepth(30000)
        }

        if(this.isBattleMode(BattleMode.CHALLENGE)  && result == BattleResult.BLUEWIN)
        {
            ChallengePage.challengeComplete(ChallengeBattlePage.level)
            SaveDataManager.save2()
        }
        if(this.isBattleMode(BattleMode.RANKBATTLE))
        {
            let count = this.getLeftCount()
            if(DEBUG_MODE)
            {
                console.log(this.enemies)
            }
            RankBattlePage.updateRank(count.allyNum,count.enemyNum)
        }
    }

    isBattleMode(battleMode:BattleMode)
    {
        return BattleScene.battleMode == battleMode
    }

    getOpponents(character:Character)
    {
        if (this.allies.includes(character))
        {
            return this.enemies
        }
        if (this.enemies.includes(character))
        {
            return this.allies
        }
        console.error("BattleScene.getOpponentsでキャラが味方でも敵でも無い")
        return []
    }
}

class DebugPage extends AbstractBattleScene
{
    static form:HTMLInputElement
    static rankForm:HTMLInputElement

    constructor()
    {
        super({key:getSceneName(PageIndex.DEBUG)})
    }

    static createHTML()
    {
        if(!DEBUG_MODE)return
        
        let page = HTML.getPageElement(PageIndex.DEBUG)
        {
            let o = HTML.createHTMLElement("input",240,120,432,64,page) as HTMLInputElement
            DebugPage.form = o
        }
        {
            let o = HTML.createHTMLElement("button",240,200,480-32,48,page) as HTMLButtonElement
            o.innerText = "サーチ"
            o.onclick = () => {
                SearchTool.search2(DebugPage.form.value)
                SearchTool.search3(DebugPage.form.value,3400)
            }
        }
        {
            let o = HTML.createHTMLElement("input",240,300,432,64,page) as HTMLInputElement
            o.placeholder = "ランクポイントを設定"
            DebugPage.rankForm = o
            o.addEventListener("keydown",(e)=>{
                if (e.key == "Enter")
                {
                    DebugPage.setRank()
                }
            })
        }
    }

    static setRank()
    {
        RankBattlePage.rank = parseInt(DebugPage.rankForm.value)
    }
}

class RankBattlePage extends Phaser.Scene
{
    static allyArea:HTMLTextAreaElement
    static rank:integer = 2000
    static maxRank:integer = 2000
    static rankText:StatusText
    static maxRankText:StatusText

    constructor()
    {
        super({key:getSceneName(PageIndex.RANKBATTLE)})
    }

    create()
    {
        RankBattlePage.rankText = new StatusText(this,"ランクポイント",300,"    "+RankBattlePage.rank.toString())
        RankBattlePage.maxRankText = new StatusText(this,"最高ランクポイント",300+32,"    "+RankBattlePage.maxRank.toString())
        {
            let o = this.add.sprite(245,300,"sprites",64)
            o.setScale(4)
        }
        {
            let o = this.add.sprite(290,300+32,"sprites",64)
            o.setScale(4)
        }
    }

    static createHTML()
    {
        let page = HTML.getPageElement(PageIndex.RANKBATTLE)
        RankBattlePage.allyArea = HTML.createHTMLElement("textarea",240,32+96+48,432,192,page) as HTMLTextAreaElement
        RankBattlePage.allyArea.placeholder = "味方の名前１\n味方の名前２\n味方の名前３\n味方の名前４"

        {
            let o = HTML.createHTMLElement("button",360,32+192+32+192+48,160,64,page) as HTMLButtonElement
            o.innerText = "戦闘開始"
            o.onclick = RankBattlePage.startBattle
        }

    }

    static getCharacters()
    {
        let o = RankBattlePage.allyArea.value.split("\n")
        return o.slice(0,4)
    }

    static startBattle()
    {
        HTML.gotoScene("battle")
        BattleScene.allyNames = RankBattlePage.getCharacters()
        BattleScene.enemyNames = RankBattlePage.getEnemies(RankBattlePage.rank)
        BattleScene.battleMode = BattleMode.RANKBATTLE
    }

    static getRandomName()
    {
        let prefix = Phaser.Utils.Array.GetRandom(PREFICES)
        let _name = Phaser.Utils.Array.GetRandom(NAMES)
        let name = prefix+_name
        return name.substring(0,12)
    }

    static getEnemies(rank:integer)
    {
        let enemies = []
        let enemyRank = Math.max(rank,2000)
        let enemyNum = 4
        if(rank<2000) enemyNum = Math.max(1,Math.ceil(rank/500))
        if(rank>5000) enemyNum = Math.ceil(rank/1250)
        let leftRank = rank - Math.floor(rank/1250)*1250
        for(let i = 0; i < enemyNum; i++)
        {
            if(i == enemyNum-1)
            {
                enemies.push(RankBattlePage.getEnemy(leftRank*4))
            }
            else
            {
                enemies.push(RankBattlePage.getEnemy(rank))
            }
        }
        return enemies
    }

    static getEnemy(rank:integer)
    {
        let enemyRank = Math.min(Math.max(rank,2000),5000)
        while(true)
        {
            let name = RankBattlePage.getRandomName()
            let val = CharacterValueCalculator.calc(name)
            if(val <= enemyRank+200 && val >= enemyRank-200)
            {
                return name
            }
        }
    }

    static getCharacterValue(o:Character)
    {
        let st = o.getModifiedStatus()
        return Math.floor(st.mhp/4)+st.mmp+st.atk+st.def+st.mag+st.mdef+st.spd
    }

    static updateRank(allyNum:integer,enemyNum:integer)
    {
        let delta = allyNum-enemyNum
        let rp = 0
        if(delta >= 4) rp = 100
        else if(delta <= -4) rp = -100
        else rp = delta*25
        RankBattlePage.rank += rp
        RankBattlePage.maxRank = Math.max(RankBattlePage.maxRank,RankBattlePage.rank)
        SaveDataManager.save2()
    }
}

class MemoPage extends Phaser.Scene
{
    static memoArea:HTMLTextAreaElement

    constructor()
    {
        super({key:"Memo"})
    }

    static createHTML()
    {
        let page = HTML.getPageElement(PageIndex.MEMO)

        // let div = HTML.createHTMLElement("div",240,16,480,720-128-32,page) as HTMLDivElement
        // div.classList.add("scrollBox")

        MemoPage.memoArea = HTML.createHTMLElement("textarea",240,32*9,432,32*16,page) as HTMLTextAreaElement
        MemoPage.memoArea.placeholder = "名前などのメモ(保存されます)"
        MemoPage.memoArea.classList.add("memo")
        MemoPage.memoArea.rows = 12
        MemoPage.memoArea.cols = 60
        MemoPage.memoArea.addEventListener("input",()=>{
            MemoPage.memoArea.value = MemoPage.memoArea.value.replace(/\//g,"／")
            MemoPage.memoArea.value = SaveDataManager.decodeMemo(SaveDataManager.encodeMemo(MemoPage.memoArea.value).substring(0,17*60))
            SaveDataManager.save2()
        })
    }

    static getStringByteCount(str: string) {
        return new Blob([str]).size
    }
}

class NonePage extends Phaser.Scene
{
    constructor()
    {
        super({key:"None"})
    }
}

class FirstScene extends Phaser.Scene
{
    constructor()
    {
        super({key:"First"})
    }

    preload()
    {
        // this.load.image("tiles", "assets/tilemap.png")
        this.load.spritesheet("avatars", "assets/avatars.png", {frameWidth:16,frameHeight:16})
        this.load.spritesheet("weapons", "assets/weapons.png", {frameWidth:16,frameHeight:16})
        this.load.spritesheet("sprites", "assets/sprites.png", {frameWidth:16,frameHeight:16})
        this.load.spritesheet("numbers", "assets/numbers.png", {frameWidth:7,frameHeight:9})
        this.load.spritesheet("texts","assets/texts.png", {frameWidth:128,frameHeight:8})
        this.load.audio("heal","assets/heal.wav")
        this.load.audio("hit","assets/hit.wav")
        this.load.audio("fireball","assets/fireball.wav")
        this.load.audio("icicle","assets/icicle.wav")
        this.load.audio("icicle_cast","assets/icicle_cast.wav")
        this.load.audio("thunder","assets/thunder.mp3")
        this.load.audio("meditation","assets/meditation.wav")
        this.load.audio("beam1","assets/beam1.wav")
        this.load.audio("beam2","assets/beam2.wav")
        this.load.audio("powerup","assets/powerup.wav")
        this.load.audio("powerdown","assets/powerdown.mp3")
        this.load.audio("bubble","assets/bubble.wav")
        this.load.audio("crack","assets/crack.wav")
        this.load.audio("reflect","assets/reflect.mp3")
        this.load.audio("drain","assets/drain.wav")
        this.load.audio("dispel","assets/dispel.wav")
        this.load.audio("twinkle","assets/twinkle.mp3")
        this.load.audio("padlock","assets/padlock.wav")
        this.load.audio("venom","assets/venom.wav")
        HTML.init()
        
    }

    update()
    {
        HTML.selectPage(PageIndex.CREATE)
    }
}

class HTML
{
    static menuButtons:HTMLButtonElement[] = []

    static pageSelected:integer = 0
    static pageElements:HTMLDivElement[] = []
    
    static init()
    {
        HTML.setStyles()
        HTML.createPages()
        HTML.createMenuButtons()
        CreatePage.createHTML()
        BattlePage.createHTML()
        ChallengePage.createHTML()
        ChallengeBattlePage.createHTML()
        RankBattlePage.createHTML()
        MemoPage.createHTML()
        DebugPage.createHTML()

        SaveDataManager.load2()

        ChallengePage.setChallengeLock()
    }

    static setStyles()
    {
        let o = document.createElement("style")
        o.innerHTML = `
        * {
            position: absolute;
            outline: none;
            border: none;
            transform: translate(-50%,-50%);
            font-size: 32px;
        }
        canvas {
            transform: none;
        }
        input {
        }
        button {
            background-color: #475c8f;
            color: white;
            border: 4px outset #2f4270;
        }
        textarea {
            resize: none;
        }
        textarea.memo {
            overflow: scroll;
        }
        button:hover {
            background-color: #6378b0;
            border: 4px outset #4b5670;
        }
        button:active {
            background-color: #b0c1eb;
            border: 4px outset #666b75;
        }
        button.menu {
            transform: none;   
        }
        div.scrollBox {
            overflow: scroll;
            transform: translate(-50%,0%);
            overflow-x: hidden;
        }
        button.complete {
            background-color: #ebce13;
            border: 4px outset #db910f;
        }
        button.complete:hover {
            background-color: #eddc6d;
            border: 4px outset #e3af54;
        }
        button.complete:active {
            background-color: #f0e7af;
            border: 4px outset #dec18e;
        }
        button.challenge {
            font-size: 24px;
        }
        button.challenge.lock {
            background-color: #939696;
            border: none;
        }
        `
        document.documentElement.appendChild(o)
    }

    static createHTMLElement(elementType:string,x:integer,y:integer,w:integer,h:integer,parent:HTMLElement): HTMLElement
    {
        let o = document.createElement(elementType)
        o.style.left = `${x}px`
        o.style.top = `${y}px`
        o.style.width = `${w}px`
        o.style.height = `${h}px`
        // HTML.pageElements[pageIndex].appendChild(o)
        parent.appendChild(o)
        if (o instanceof HTMLButtonElement)
        {
            o.innerText = "ー"
        }
        if (o instanceof HTMLInputElement)
        {
            o.placeholder = "名前を入力";
            o.maxLength = 12;
            o.minLength = 1;
            
            o.type = "text"
        }
        if (o instanceof HTMLTextAreaElement)
        {
            o.placeholder = "名前を入力";
            o.cols = 12
            o.rows = 4
        }
        return o
    }

    static setHTMLActive(o:HTMLElement, isActive: boolean)
    {
        if (isActive)
        {
            o.style.display = "initial"
        }
        else
        {
            o.style.display = "none"
        }
    }

    static selectPage(index:integer)
    {   
        for (let i = 0; i < HTML.pageElements.length; i++)
        {
            HTML.setHTMLActive(HTML.pageElements[i], index==i)
        }
        let currentSceneName = game.scene.getScenes(true)[0]
        let nextSceneName = "None"
        if (game.scene.keys[getSceneName(index)])
        {
            nextSceneName = getSceneName(index)
            HTML.pageSelected = index
        }
        else
        {
            HTML.pageSelected = -1
        }
        game.scene.getScene(currentSceneName).scene.start(nextSceneName)

        
    }

    static gotoScene(nextSceneName:string)
    {
        for (let i = 0; i < HTML.pageElements.length; i++)
        {
            HTML.setHTMLActive(HTML.pageElements[i], false)
        }
        let currentSceneName = game.scene.getScenes(true)[0]
        game.scene.getScene(currentSceneName).scene.start(nextSceneName)
    }

    static createPages()
    {
        for (let i = 0; i < 7; i++)
        {
            let div = document.createElement("div")
            div.classList.add("page"+i)
            document.body.appendChild(div)
            HTML.pageElements.push(div)
        }
    }

    static createButton(x:integer,y:integer,w:integer,h:integer,parent:HTMLElement)
    {
        let o = document.createElement("button")
        o.style.left = `${x}px`
        o.style.top = `${y}px`
        o.style.width = `${w}px`
        o.style.height = `${h}px`
        o.innerText = "ー"
        parent.appendChild(o)
        return o
    }

    static createMenuButtons()
    {
        HTML.menuButtons = []
        let w = 160
        let h = 64
        for (let y of [720-2*h,720-h])
        {
            for (let x of [0,160,320])
            {
                let o = HTML.createButton(x,y,w,h,document.body)
                o.classList.add("menu")
                
                HTML.menuButtons.push(o)
            }
        }
        for (let i = 0; i < HTML.menuButtons.length; i++)
        {
            let o = HTML.menuButtons[i]
            o.onclick = () => {
                HTML.selectPage(i)
            }
        }
        HTML.menuButtons[PageIndex.CREATE].innerText = "作成"
        HTML.menuButtons[PageIndex.BATTLE].innerText = "対戦"
        HTML.menuButtons[PageIndex.CHALLENGE].innerText = "挑戦"
        HTML.menuButtons[PageIndex.RANKBATTLE].innerText = "ランク戦"
        HTML.menuButtons[PageIndex.MEMO].innerText = "メモ"

        if(DEBUG_MODE)HTML.menuButtons[PageIndex.DEBUG].innerText = "デバッグ"
        
    }

    static getPageElement(pageIndex:integer)
    {
        return HTML.pageElements[pageIndex]
    }


}

function getSceneName(pageIndex:PageIndex): string
{
    return `page-${pageIndex}`
}

enum AvatarType
{
    HAIR,
    HEAD,
    TOPS,
    BOTTOMS,
    BOOTS,
}

function getAvatarSpriteFrame(type:AvatarType,index:integer)
{
    let frame = type*20 + index
    if(type == AvatarType.BOTTOMS || type == AvatarType.BOOTS) frame = type*20 + index*4
    return frame
}

enum WeaponType
{
    SWORD,
    BOW,
    STAFF,
    GUN,
}

function getWeaponSpriteFrame(type:WeaponType,index:integer)
{
    let frame = type*10 + index
    return frame
}

const config = {
    type: Phaser.AUTO,
    parent: 'phaser-example',
    width: 480,
    height: 720,
    pixelArt: true,
    // backgroundColor: '#304858',
    backgroundColor: "#35576e",
    scene: [FirstScene,CreatePage,BattlePage,BattleScene,ChallengePage,RankBattlePage,MemoPage,DebugPage,ChallengeBattlePage,NonePage],
    physics: {
        default: "arcade",
        arcade: {
            gravity: { x: 0, y: 0 },
            debug: false,
            fps: 600,
        },
    },
}

const game = new Phaser.Game(config)



/*================================================================================

ACTION

=================================================================================*/

enum TargetType
{
    NONE,
    SELF,
    ALLY,
    ENEMY,
    ALLYALL,
    ENEMYALL,
    ALL,
}
new Phaser.Scene()
interface IBattleField
{
    getOpponents(character:Character): Character[]
}

abstract class Action
{
    me:Character
    field:AbstractBattleScene
    type:TargetType = TargetType.ENEMY
    range = 0
    consumeMP = 0
    name = "スキル名"

    constructor(character:Character,battleField:AbstractBattleScene,mt?:MersenneTwister)
    {
        this.me = character
        this.field = battleField
    }
    distanceTo(o:Character)
    {
        return Phaser.Math.Distance.Between(this.me.x,this.me.y,o.x,o.y)
    }

    getTargetsInRange():Character[]
    {
        let targets:Character[] = []
        switch (this.type)
        {
            case TargetType.NONE: targets = []; break;
            case TargetType.SELF: targets = [this.me]; break;
            case TargetType.ENEMY: targets = this.field.getOpponents(this.me); break;
            case TargetType.ALLY: targets = this.field.getAllies(this.me); break;
            default: console.error("Actionの未定義のTargetType")

        }
        return targets.filter(o => this.distanceTo(o) <= this.range)
    }

    getLivingsInRange():Character[]
    {
        return this.getTargetsInRange().filter(o => o.hp > 0)
    }

    isBuffLimited(o:Character,buffClass:any)
    {
        for(let key in o.buffs)
        {
            let buff = o.buffs[key]
            if(buff instanceof buffClass.prototype.constructor)
            {
                if(buff.stackable) 
                {
                    if(buff.stackNum >= buff.stackMax) return true
                }
                else return true
            }
        }
        return false
    }

    getLivingsInRangeHasNoBuff(buffClass:any)
    {
        return this.getLivingsInRange().filter((o)=>{
            return !this.isBuffLimited(o,buffClass)
        })
    }

    setDirectionToward(o:Character)
    {
        let d = o.x-this.me.x
        if (d > 0)
        {
            this.me.setFlipX(false)
        }
        else if (d < 0)
        {
            this.me.setFlipX(true)
        }
    }

    // ここを変更
    getMeetConditionTargets()
    {
        return this.getLivingsInRange()
    }

    isMeetCondition()
    {
        if(this.me.mp < this.consumeMP) return false
        if (this.type == TargetType.NONE)
        {
            return true
        }
        return this.getMeetConditionTargets().length > 0
    }

    actionTemplate()
    {
        let actionDid = true
        if(!this.isMeetCondition())return
        let st = this.me.getModifiedStatus()
        if(!st.actionTargetAI)return

        if(this.type == TargetType.NONE)
        {
            this.me.consumeMP(this.consumeMP)
            this.actionToNull()
        }
        else
        {
            let target = st.actionTargetAI.choose(this.getMeetConditionTargets())
            if(target != null)
            {
                this.me.consumeMP(this.consumeMP)
                this.actionToTarget(target)
            }
            else
            {
                actionDid = false
            }
        }

        if(actionDid)
        {
            st.actionAI?.changeActionAIAfterDo()
        }
    }

    actionToTarget(target:Character)
    {

    }

    actionToNull()
    {

    }
}

class AttackWithWeapon extends Action
{
    range = 40
    type = TargetType.ENEMY
    weapon: Weapon
    name = "攻撃"
    

    constructor(character:Character,battleField:AbstractBattleScene,weapon:Weapon)
    {
        super(character,battleField)
        this.weapon = weapon
        this.range = weapon.range    
    }

    actionToTarget(target:Character)
    {
        this.setDirectionToward(target)

        let o:Bullet|null
        let weaponType = this.me.weapon.weaponType
        if(weaponType==WeaponType.BOW)
        {
            let st = this.me.getModifiedStatus()
            let bAtk = Math.floor(st.atk*0.8)
            o = new Bullet(this.me,this.field,0,bAtk,0)
            o.sprite.setPosition(this.me.x+this.me.getFlipX(8),this.me.y-4)
            
            let vec = new Phaser.Math.Vector2(target.x-o.sprite.x,target.y-o.sprite.y)
            let spread = Math.random()*this.me.weapon.bulletSpread
            vec = vec.setLength(Math.max(50,st.atk/5)).rotate(-spread/2+spread*Math.random())
            o.sprite.setVelocity(vec.x,vec.y)
        }
        else if (weaponType==WeaponType.GUN)
        {
            let st = this.me.getModifiedStatus()
            let bAtk = Math.floor(st.atk*0.8)
            o = new Bullet(this.me,this.field,35,bAtk,0)
            o.sprite.setPosition(this.me.x+this.me.getFlipX(8),this.me.y-4)
            
            let vec = new Phaser.Math.Vector2(target.x-o.sprite.x,target.y-o.sprite.y)
            let spread = Math.random()*this.me.weapon.bulletSpread
            vec = vec.setLength(Math.max(250,st.atk*2/5)).rotate(-spread/2+spread*Math.random())
            o.sprite.setVelocity(vec.x,vec.y)
        }
        else
        {
            o = new WeaponBullet(this.me,this.field)
        }

        // 共通処理
        if(o != null)
        {
            o.enchants = this.me.weapon.enchants
        }
    }

}

class Heal extends Action
{
    type = TargetType.ALLY
    range = 250
    consumeMP = 50
    name = "ヒール"

    getMeetConditionTargets()
    {
        let st = this.me.getModifiedStatus()
        return this.getLivingsInRange().filter(o => o.hp < st.mhp)   
    }

    actionToTarget(target:Character)
    {
        // let point = calcMagAttackPoint(this.me)
        // point = calcMagDmg(point,target)
        let st = this.me.getModifiedStatus()
        let point = DamageObject.calcDmgPoint(st.mag,st.mdef)
        target.hp += point
        for(let i = 0; i < 5; i++)
        {
            let e = new Effect(this.field,target.x-16+Math.random()*32,target.y-16+Math.random()*32,2)
            e.vy = -0.1-Math.random()*0.2
        }
        this.field.sound.play("heal")
    }

}

class FireBall extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 50
    name = "ファイアボール"

    actionToTarget(target:Character)
    {
        let st = this.me.getModifiedStatus()
        let bMag = st.mag
        let o = new Bullet(this.me,this.field,3,0,bMag)
        o.sprite.setPosition(this.me.x+this.me.getFlipX(8),this.me.y-4)
        o.hitSound = "fireball"
        
        let vec = new Phaser.Math.Vector2(target.x-o.sprite.x,target.y-o.sprite.y)
        vec = vec.normalize().scale(100+st.mag/5)
        o.sprite.setVelocity(vec.x,vec.y)
        this.field.sound.play("fireball")

    }
}

class IcicleRain extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 150
    name = "アイシクルレイン"

    actionToTarget(target:Character)
    {
        for(let i = 0; i < 5; i++)
        {
            // let point = calcMagAttackPoint(this.me)
            let st = this.me.getModifiedStatus()
            let bMag = st.mag
            let o = new Bullet(this.me,this.field,4,0,bMag)
            o.sprite.setPosition(target.x-32+Math.random()*64,target.y-48-Math.random()*8)
            o.hitSound = "icicle"
            o.setDelay(Math.random()*180)
            o.autoRotAlongVelocity = false
            o.sprite.setAccelerationY(50)
            o.leftTime = o.delayTime + 180
            o.enchants = [new FreezeEnchant(30)]
        }
        this.field.sound.play("icicle_cast")

    }
}

class Beam extends Action
{
    range = 300
    type = TargetType.ENEMY
    consumeMP = 200
    name = "ビーム"

    actionToTarget(target:Character)
    {
        new BeamHead(this.me,this.field,target)
        this.field.sound.play("beam2")
    }
}

class BubbleBreath extends Action
{
    range = 150
    type = TargetType.ENEMY
    consumeMP = 150
    name = "バブルブレス"
    
    actionToTarget(target:Character)
    {
        for(let i = 0; i < 15; i++)
        {
            let o = new FloatingBubble(this.me,this.field)
            o.hitSound = "bubble"
            let vec = new Phaser.Math.Vector2(target.x-this.me.x,target.y-this.me.y).setLength(40+Math.random()*20).rotate(-1/2+Math.random())
            o.sprite.body.setVelocity(vec.x,vec.y)
            
        }
        this.field.sound.play("bubble")
    }
}

class LifeDrain extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 100
    name = "ドレイン"

    actionToTarget(target:Character)
    {
        let st = this.me.getModifiedStatus()
        let bMag = st.mag
        let o = new Bullet(this.me,this.field,33,0,bMag)
        o.enchants = [new HPAbsorbEnchant(1)]
        o.hitSound = "drain"
        o.autoRotAlongVelocity = false
        o.sprite.setAngularVelocity(-200)
        let vec = new Phaser.Math.Vector2(target.x-this.me.x,target.y-this.me.y).setLength(50+st.mag/10)
        o.sprite.body.setVelocity(vec.x,vec.y)
    }
}

class ManaDrain extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 100
    name = "マナドレイン"

    actionToTarget(target:Character)
    {
        let st = this.me.getModifiedStatus()
        let bMag = st.mag
        let o = new Bullet(this.me,this.field,34,0,0)
        o.enchants = [new MPAbsorbEnchant(1)]
        o.mpAtk = bMag
        o.hitSound = "drain"
        o.autoRotAlongVelocity = false
        let vec = new Phaser.Math.Vector2(target.x-this.me.x,target.y-this.me.y).setLength(50+st.mag/10)
        o.sprite.body.setVelocity(vec.x,vec.y)
        o.sprite.setAngularVelocity(-200)
    }
}

class SummonThunderClouds extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 300
    name = "サンダークラウド"

    actionToTarget(target:Character)
    {
        new ThunderCloud(this.me,this.field,target)
        // this.field.sound.play("icicle_cast")

    }
}

class CallAllies extends Action
{
    range = 0
    type = TargetType.NONE
    consumeMP = 500
    name = "仲間を呼ぶ"
    charaName = ""

    constructor(character:Character,battleField:AbstractBattleScene,mt:MersenneTwister)
    {
        super(character,battleField,mt)

        let jobs:string[] = ["村人","村娘","戦士","兵士","騎士","盗賊","船乗り","通行人"]
        this.charaName += jobs[mt.int()%jobs.length]
        for (let i = 0; i < 8; i++)
        {
            this.charaName += String.fromCharCode(mt.int()%94 + 33)
        }

        this.name += `(${this.charaName})`
        
    }

    actionToNull()
    {
        let o = new Character(this.me.scene,this.charaName,this.me.currentTeam)
        o.y = this.me.y
        if(this.me.currentTeam == Team.ALLY) o.x = 0
        else if (this.me.currentTeam == Team.ENEMY) o.x = 480
        o._hp = Math.floor(o._hp/4)
        o._mp = 0

    }
}

class Meditation extends Action
{
    range = 0
    type = TargetType.SELF
    consumeMP = 0
    name = "瞑想"

    getMeetConditionTargets()
    {
        let st = this.me.getModifiedStatus()
        return [this.me].filter(o => o.mp < st.mmp)
    }

    actionToTarget(target:Character)
    {
        let point = 20 + Math.random()*5
        target.mp += point
        for(let i = 0; i < 3; i++)
        {
            let e = new Effect(this.field,target.x-16+Math.random()*32,target.y-16+Math.random()*32,8)
            e.vy = -0.1-Math.random()*0.2
        }
        this.field.sound.play("meditation")
    }
}

class StatusBoost extends Action
{
    range = 250
    type = TargetType.ALLY
    consumeMP = 100
    name = "ブースト"

    statusType:StatusType

    constructor(character:Character,battleField:AbstractBattleScene,mt:MersenneTwister)
    {
        super(character,battleField,mt)
        this.statusType = mt.int()%7 as StatusType
        
        let typeName = ["ライフ","マナ","アタック","ディフェンス","スピード","マジック","マインド"][this.statusType]
        this.name = typeName+"ブースト"
    }

    getMeetConditionTargets()
    {
        return this.getLivingsInRange()
    }

    actionToTarget(target:Character)
    {
        let point = Math.floor(calcMagAttackPoint(this.me)/5)
        let buff = new StatusAddBuff(target,this.field,3600,this.statusType,point)
        {
            let o = new Effect(this.field,target.x,target.y-24,11+this.statusType)
            o.vy = -0.2
            o.leftTime = 240
        }
        for(let i = 0; i < 8; i++)
        {
            let e = new Effect(this.field,target.x-16+Math.random()*32,target.y+24,18)
            e.vy = -0.3-Math.random()*0.6
            e.setDelay(Math.random()*i*10)
            e.leftTime += i*10
        }
        this.field.sound.play("powerup")
    }
}

class StatusDown extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 50
    name = "ダウン"

    statusType:StatusType

    constructor(character:Character,battleField:AbstractBattleScene,mt:MersenneTwister)
    {
        super(character,battleField,mt)
        this.statusType = mt.int()%7 as StatusType
        
        let typeName = ["ライフ","マナ","アタック","ディフェンス","スピード","マジック","マインド"][this.statusType]
        this.name = typeName+"ダウン"
    }

    actionToTarget(target:Character)
    {
        let point = -Math.floor(calcMagAttackPoint(this.me)/5)
        let buff = new StatusAddBuff(target,this.field,3600,this.statusType,point)
        {
            let o = new Effect(this.field,target.x,target.y-48,21+this.statusType)
            o.vy = 0.2
            o.leftTime = 240
        }
        for(let i = 0; i < 8; i++)
        {
            let e = new Effect(this.field,target.x-16+Math.random()*32,target.y-64,28)
            e.vy = 0.3+Math.random()*0.6
            e.setDelay(Math.random()*i*10)
            e.leftTime += i*10
        }
        this.field.sound.play("powerdown")
    }
}

class SpiritBless extends Action
{
    range = 150
    type = TargetType.ALLY
    consumeMP = 100
    name = "精霊の加護"

    getMeetConditionTargets()
    {
        return this.getLivingsInRange().filter((o)=>{
            for(let key in o.buffs)
            {
                let buff = o.buffs[key]
                if(buff instanceof SpiritBuff && buff.stackNum == buff.stackMax)
                {
                    return false
                }
            }
            return true
        })   
    }

    actionToTarget(target:Character)
    {
        let buff = new SpiritBuff(target,this.field)
        this.field.sound.play("bubble")
    }
}

class Reflect extends Action
{
    range = 250
    type = TargetType.ALLY
    consumeMP = 200
    name = "リフレクト"

    getMeetConditionTargets()
    {
        return this.getLivingsInRange().filter((o)=>{
            for(let key in o.buffs)
            {
                let buff = o.buffs[key]
                if(buff instanceof ReflectBuff)
                {
                    return false
                }
            }
            return true
        })
    }

    actionToTarget(target:Character)
    {
        let buff = new ReflectBuff(target,this.field,1800)
        this.field.sound.play("reflect")
    }
}

class Dispel extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 100
    name = "ディスペル"

    getMeetConditionTargets()
    {
        return this.getLivingsInRange().filter((o)=>{
            for(let key in o.buffs)
            {
                let buff = o.buffs[key]
                if(buff.buffType == BuffType.Buff)
                {
                    return true
                }
            }
            return false
        })
    }

    actionToTarget(target:Character)
    {
        for(let key in target.buffs)
        {
            let buff = target.buffs[key]
            if(buff.buffType == BuffType.Buff)
            {
                buff.destroy()
            }
        }
        for(let i = 0; i < 8;i++)
        {
            let o = new Effect(this.field, target.x,target.y, 36)
            let vec = new Phaser.Math.Vector2(0,-1).setLength(2).rotate(Math.PI/4*i)
            o.vx = vec.x
            o.vy = vec.y
            o.leftTime = 25
            o.updateFunc = (o:Effect,t:integer) => {
                let a = 3 - t*0.12
                o.sprite.setAlpha(a)
            }
        }
        this.field.sound.play("dispel")
    }
}

class Clearance extends Action
{
    range = 250
    type = TargetType.ALLY
    consumeMP = 100
    name = "クリアランス"

    getMeetConditionTargets()
    {
        return this.getLivingsInRange().filter((o)=>{
            for(let key in o.buffs)
            {
                let buff = o.buffs[key]
                if(buff.buffType == BuffType.Debuff)
                {
                    return true
                }
            }
            return false
        })
    }

    actionToTarget(target:Character)
    {
        for(let key in target.buffs)
        {
            let buff = target.buffs[key]
            if(buff.buffType == BuffType.Debuff)
            {
                buff.destroy()
            }
        }
        for(let i = 0; i < 12;i++)
        {
            let o = new Effect(this.field, target.x,target.y, 38)
            let vec = new Phaser.Math.Vector2(0,1).setLength(24).rotate(Math.PI/6*i)
            o.sprite.setPosition(target.x+vec.x,target.y+vec.y)
            o.leftTime = 35+5*i
            o.setDelay(5*i)
            o.updateFunc = (o:Effect,t:integer) => {
                if(!o.delayFlag) o.sprite.setRotation(-t/15)
            }
        }
        this.field.sound.play("twinkle")
    }
}

class Cover extends Action
{
    range = 150
    type = TargetType.ALLY
    consumeMP = 25
    name = "かばう"

    getMeetConditionTargets()
    {
        let st = this.me.getModifiedStatus()
        return this.getLivingsInRange().filter((o)=>{
            if(o == this.me)return false
            if(o.hp >= this.me.hp)return false
            if(o.hp < 100)return false

            for(let key in o.buffs)
            {
                let buff = o.buffs[key]
                if(buff instanceof CoverBuff)
                {
                    return false
                }
            }
            return true
        })   
    }
    
    actionToTarget(target: Character)
    {
        new CoverBuff(target,this.field,900,this.me)
    }
}

class VenomShot extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 50
    name = "ヴェノム"
    
    getMeetConditionTargets()
    {
        return this.getLivingsInRangeHasNoBuff(PoisonDebuff)
        
    }

    actionToTarget(target: Character)
    {
        let st = this.me.getModifiedStatus()
        let bMag = Math.floor(st.mag/2)
        let o = new Bullet(this.me,this.field,39,0,bMag)
        o.shotTo(target.x,target.y,100+st.mag/5)
        let time = Math.max(60,Math.floor(st.mag))
        o.enchants = [new PoisonEnchant(time)]
        o.hitSound = "venom"
        this.field.sound.play("venom")
    }
}

class SkillLock extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 150
    name = "スキルロック"

    getMeetConditionTargets()
    {
        return this.getLivingsInRangeHasNoBuff(SkillLockDebuff)   
    }

    actionToTarget(target: Character)
    {
        new SkillLockDebuff(target,this.field,900)
        {
            let o = new Effect(this.field,target.x,target.y,62)
            o.updateFunc = (self:Effect,t:integer) => {
                if(t<54)
                {
                    self.x = target.x
                    self.y = target.y
                }
                else if(t>=54)
                {
                    self.x = target.x + Math.floor(Math.sin(t*Math.PI/2))
                    o.y = target.y
                }
                if(t==30) self.field.sound.play("padlock")
            }
            o.leftTime += 10
            o.sprite.setDepth(o.sprite.depth+1)
        }
        {
            let o = new Effect(this.field,target.x,target.y-4,63)
            o.updateFunc = (self:Effect,t:integer) => {
                
                if(t<30)
                {
                    o.x = target.x
                    o.y = target.y-8-24+t*0.8
                }
                else if(t<54)
                {
                    o.x = target.x
                    o.y = target.y-8
                }
                else if(t>=54)
                {
                    self.x = target.x + Math.floor(Math.sin(Math.floor(t/2)*Math.PI/2))
                    o.y = target.y-8
                }
            }
        }
        

    }
}

class Regenerate extends Action
{
    range = 250
    type = TargetType.ENEMY
    consumeMP = 150
    name = "リジェネレート"

    getMeetConditionTargets()
    {
        return this.getLivingsInRangeHasNoBuff(RegenerateBuff)   
    }

    actionToTarget(target: Character)
    {
        let st = this.me.getModifiedStatus()
        let time = Math.max(60,Math.floor(st.mag))
        new RegenerateBuff(target,this.field,time)
        for(let i = 0; i < 5; i++)
        {
            let e = new Effect(this.field,target.x-16+Math.random()*32,target.y-16+Math.random()*32,67)
            e.vy = -0.1-Math.random()*0.2
        }
        this.field.sound.play("heal")
        
    }
}

/*================================================================================

ACTION AI : どのActionをするかを決めるAI

=================================================================================*/

abstract class ActionAI
{
    name = "戦略の名前"
    me:Character
    field:IBattleField

    constructor(character:Character,battleField:IBattleField,mt:MersenneTwister)
    {
        this.me = character
        this.field = battleField
    }

    getPossibleActions():Action[]
    {
        return this.me.actions.filter(o => o.isMeetCondition())
    }

    changeActionAIAfterDo()
    {
    }

    abstract choose():Action|null
}

class RandomActionAI extends ActionAI
{
    name = "ランダム"
    choose()
    {
        let actions = this.getPossibleActions()
        if(!actions || actions.length == 0) return null
        return actions[Math.floor(Math.random()*actions.length)]
    }
}

class WeightActionAI extends ActionAI
{
    name = "固定比率"
    weights:integer[] = []

    constructor(character:Character,battleField:IBattleField,mt:MersenneTwister)
    {
        super(character,battleField,mt)
        if(mt)
        {
            for(let i = 0; i < 4; i++)
            {
                this.weights.push(mt.int()%100)
            }
        }
        this.name = this.name + `(${this.weights.join("/")})`
    }

    choose()
    {
        let actions = this.getPossibleActions()
        if(!actions || actions.length == 0) return null
        let actionIndeces = actions.map(o => this.me.actions.indexOf(o))
        let ws = actionIndeces.map(i => this.weights[i])
        let r = 1 + Math.floor(Math.random()*ws.reduce((sum,o)=>sum+o,0))
        let sum = 0
        let index = 0
        for(let i = 0; i < ws.length;i++)
        {
            sum += ws[i]
            if(r <= sum)
            {
                index = i
                break
            }
        }
        return actions[index]
    }
}

class GoalSettingActionAI extends ActionAI
{
    //バランスを取る
    name = "目標設定"
    goalAction:Action|null = null

    choose()
    {
        let actions = this.getPossibleActions()
        if(!actions || actions.length == 0) return null

        let st = this.me.getModifiedStatus()
        // もし、ステ変更により、目標スキルの消費MPがMMPを超えたら、目標をリセット。
        if (this.goalAction)
        {
            if (this.goalAction.consumeMP > st.mmp) this.goalAction = null
        }
        if (!this.goalAction)
        {
            let possibleMMPActions = this.me.actions.filter(o => o.consumeMP <= st.mmp)
            this.goalAction = Phaser.Utils.Array.GetRandom(possibleMMPActions)
        }
        if (actions.includes(this.goalAction))
        {
            return this.goalAction
        }
        let leastMPConsume = actions.reduce((min,o) => {
            return o.consumeMP < min.consumeMP ? o : min
        },actions[0]).consumeMP
        let leastMPConsumeActions = this.me.actions.filter(o => o.consumeMP==leastMPConsume)
        return Phaser.Utils.Array.GetRandom(leastMPConsumeActions)       
    }

    changeActionAIAfterDo()
    {
        this.goalAction = null    
    }
}

class NothingActionAI extends ActionAI
{
    name = "何も選ばない"
    choose()
    {
        return null
    }
}

class OnlyWeaponAttackActionAI extends ActionAI
{
    name = "通常攻撃のみ"
    choose()
    {
        let actions = this.getPossibleActions()
        if(!actions || actions.length == 0) return null
        for(let o of actions)
        {
            if (o instanceof AttackWithWeapon)
            {
                return o
            }
        }
        return null
    }
}



/*================================================================================

ACTION TARGET AI : 範囲内の敵からどの敵を狙うかを決めるAI

=================================================================================*/

abstract class ActionTargetAI
{
    name = "戦略の名前"
    me:Character
    field:IBattleField

    constructor(character:Character,battleField:IBattleField)
    {
        this.me = character
        this.field = battleField
    }
    getEnemyList(me:Character,field:AbstractBattleScene)
    {
        return field.getOpponents(me)
    }
    distanceTo(o:Character)
    {
        return Phaser.Math.Distance.Between(this.me.x,this.me.y,o.x,o.y)
    }

    abstract choose(characters:Character[]):Character|null
}

class ActionToRandom extends ActionTargetAI
{
    name = "ランダム"
    choose(characters:Character[])
    {
        if(characters.length==0) return null
        return characters.filter(o=>o.hp>0)[Math.floor(Math.random()*characters.length)]
    }
}

class ActionToNearest extends ActionTargetAI
{
    name = "最も近くの対象"
    choose(characters:Character[])
    {
        if(characters.length==0) return null
        return characters.filter(o=>o.hp>0).reduce((nearest,o) => {
            return this.distanceTo(nearest) > this.distanceTo(o) ? o : nearest
        })
    }
}

class ActionToLowestHP extends ActionTargetAI
{
    name = "最もHPが低い対象"
    choose(characters:Character[])
    {
        if(characters.length==0) return null
        return characters.filter(o=>o.hp>0)[Math.floor(Math.random()*characters.length)]
    }
}


/*================================================================================

MOVEAI : 動き方を決めるAI

=================================================================================*/


abstract class MoveAI
{
    name = "戦略の名前"
    me:Character
    field:IBattleField

    constructor(character:Character,battleField:IBattleField)
    {
        this.me = character
        this.field = battleField
    }

    getEnemyList()
    {
        return this.field.getOpponents(this.me)
    }

    getLivingEnemyList()
    {
        return this.field.getOpponents(this.me).filter(o => o.hp > 0)
    }

    distanceTo(o:Character)
    {
        return Phaser.Math.Distance.Between(this.me.x,this.me.y,o.x,o.y)
    }

    moveTowardCharacter(o:Character)
    {
        let d = this.distanceTo(o)

        if (d > this.me.getAtkRange())
        {
            this.me.setVelocityPointTo(o.x,o.y)
        }
        else
        {
            this.me.setVelocity(0,0)
        }
    }

    move()
    {

    }
}
class MoveToNearestEnemy extends MoveAI
{
    name = "最も近くの敵"
    getTarget()
    {
        if (this.getLivingEnemyList().length==0) return
        return this.getLivingEnemyList().reduce((nearest,o) => {
            return this.distanceTo(nearest) > this.distanceTo(o) ? o : nearest
        })
    }

    move()
    {
        let target = this.getTarget()
        if (!target) return
        this.moveTowardCharacter(target)
    }
}

class MoveToLowestHPEnemy extends MoveAI
{
    name = "最もHPが低い敵"
    getTarget()
    {
        if (this.getLivingEnemyList().length==0) return
        let target =  this.getLivingEnemyList().reduce((lowest,o) => {
            return o.hp < lowest.hp ? o : lowest
        })
        let targets = this.getLivingEnemyList().filter(o => o.hp == target.hp)
        return targets.reduce((nearest,o) => {
            return this.distanceTo(nearest) > this.distanceTo(o) ? o : nearest
        })
    }

    move()
    {
        let target = this.getTarget()
        if (!target) return
        this.moveTowardCharacter(target)
    }
}

class MoveToRangeEdge extends MoveAI
{
    name = "射程ギリギリ"
    getNearestTarget()
    {
        if (this.getLivingEnemyList().length==0) return
        return this.getLivingEnemyList().reduce((nearest,o) => {
            return this.distanceTo(nearest) > this.distanceTo(o) ? o : nearest
        })
    }
    moveAgainstCharacter(o:Character)
    {
        this.me.setVelocityPointTo(this.me.x*2-o.x,this.me.y*2-o.y)
    }
    move()
    {
        let target = this.getNearestTarget()
        if (!target) return
        let range = this.me.getAtkRange()
        let d = this.distanceTo(target)
        if (d >= range)
        {
            this.moveTowardCharacter(target)
        }
        else if (d <= range-this.me.getMoveRange())
        {
            this.moveAgainstCharacter(target)
        }
        else
        {
            this.me.setVelocity(0,0)
        }
    }
}

class DontMove extends MoveAI
{
    name = "固定"
    
    move()
    {
        this.me.setVelocity(0,0)
    }
}

/*================================================================================

BUFF

=================================================================================*/

enum StatusType
{
    HP,
    MP,
    ATK,
    DEF,
    SPD,
    MAG,
    MDEF,
}

class StatusObject
{
    mhp:number = 0
    mmp:number = 0
    atk:number = 0
    def:number = 0
    spd:number = 0
    mag:number = 0
    mdef:number = 0

    moveAI: MoveAI|null = null
    actionTargetAI: ActionTargetAI|null = null
    actionAI: ActionAI|null = null
    
    constructor()
    {
        this.mhp = 0
        this.mmp = 0
        this.atk = 0
        this.def = 0
        this.spd = 0
        this.mag = 0
        this.mdef = 0
    }
}

enum BuffType
{
    Buff,
    Debuff,
    Passive,
}

class Buff
{
    uuid:string = ""
    name = "バフの名前(stack判定で使う)"
    me:Character
    field:AbstractBattleScene
    leftTime:number = 0
    time:integer = 0
    stackable = false
    addFlag = true
    _stackNum = 1
    buffType:BuffType = BuffType.Buff
    buffIconIndex = -1
    relativeDepth = 1

    container:EffectContainer

    set stackNum(val:integer)
    {
        this._stackNum = Math.min(this.stackMax, val)
        if(this._stackNum <= 0)
        {
            this.destroy()
        }
    }
    get stackNum()
    {
        return this._stackNum
    }
    stackMax = 1

    // meとは書いてるけど、Buffを作るときは、targetを入れる
    constructor(target:Character,field:AbstractBattleScene,leftTime:number)
    {
        this.me = target
        this.field = field
        this.leftTime = leftTime
        this.container = new EffectContainer(field)
    }

    //全ての派生クラスで必ず実行する。
    //stackIconIndexの変更後に呼ぶ
    attachBuff()
    {
        if(this.stackable)
        {
            let o = this.getSameBuffOnMe()
            if(o != null)
            {
                o.stack(this)
                this.addFlag = false
            }
        }

        if(this.addFlag)
        {
            this.me.addBuff(this)
        }
    }

    getSameBuffOnMe():Buff|null
    {
        for (let key in this.me.buffs)
        {
            let o = this.me.buffs[key]
            if (this.name == o.name)
            {
                return o
            }
        }
        return null
    }
    
    // stackするときの処理
    stack(o:Buff)
    {
        this.leftTime += o.leftTime
        this.stackNum++
    }

    statusModify(status:StatusObject):StatusObject
    {
        return status
    }

    

    update()
    {
        // this.container.container.setPosition(this.me.x,this.me.y)
        this.container.update()
        this.container.container.setDepth(this.me.container.depth+this.relativeDepth)
        this.container.container.setPosition(this.me.x,this.me.y)
        this.leftTime--
        this.time++
        if(this.leftTime<=0)
        {
            this.destroy()
        }
    }

    destroy()
    {
        this.me.removeBuff(this)
        this.container.destroy()
    }

    dmgModify(dmgObj:DamageObject):void
    {
    }

    collideModify()
    {

    }
}

class StatusAddBuff extends Buff
{
    name = "ステータス変化バフ"
    type:StatusType
    val:integer

    constructor(me:Character,field:AbstractBattleScene,leftTime:number,type:StatusType,val:integer)
    {
        super(me,field,leftTime)

        this.type = type
        this.val = val
        if(this.val >= 0)
        {
            this.buffType = BuffType.Buff
            this.buffIconIndex = 40+this.type
        }
        else
        {
            this.buffType = BuffType.Debuff
            this.buffIconIndex = 50+this.type
        }
        this.attachBuff()

        

        
    }

    statusModify(status:StatusObject)
    {
        let o = status
        switch(this.type)
        {
            case StatusType.HP: o.mhp += this.val; break;
            case StatusType.MP: o.mmp += this.val; break;
            case StatusType.ATK: o.atk += this.val; break;
            case StatusType.DEF: o.def += this.val; break;
            case StatusType.SPD: o.spd += this.val; break;
            case StatusType.MAG: o.mag += this.val; break;
            case StatusType.MDEF: o.mdef += this.val; break;
        }
        return o
    }
}

class SpiritBuff extends Buff
{
    name = "精霊の加護"
    stackMax = 5
    stackable = true
    buffIconIndex = 47

    constructor(me:Character,field:AbstractBattleScene)
    {
        let leftTime = 7200
        super(me,field,leftTime)
        this.attachBuff()

        if(this.addFlag)
        {
            for(let i = 0; i < this.stackMax; i++)
            {
                let o = new Effect(field,0,0,19)
                o.sprite.setAlpha(0)
                this.container.add(o)
            }
            this.container.effects[0].sprite.setAlpha(1)

            this.container.effectUpdate = (i:integer,o:Effect) => {
                let delta = Math.PI*2/5
                let r = 32
                let rad = this.time/60 - delta*i
                o.sprite.setPosition(-r*Math.cos(rad),-r*Math.sin(rad))
                o.leftTime = 10
            }
        }
    }

    stack(o:SpiritBuff)
    {
        this.leftTime = 7200
        this.stackNum++
        this.container.effects[this.stackNum-1].sprite.setAlpha(1)
    }

    dmgModify(dmgObj: DamageObject)
    {
        this.stackNum--
        this.container.effects[this.stackNum].sprite.setAlpha(0)
        this.field.sound.play("crack")
        dmgObj.phyDmg = 0
        dmgObj.magDmg = 0
        
    }

    statusModify(status: StatusObject)
    {
        let o = status
        o.mmp += 100*this.stackNum
        o.mag += 100*this.stackNum
        return o
    }
}

class FreezeDebuff extends Buff
{
    name = "凍結"
    stackable = true
    buffType = BuffType.Debuff
    buffIconIndex = 48

    constructor(me:Character,field:AbstractBattleScene,leftTime:number)
    {
        super(me,field,leftTime)
        this.attachBuff()

        if(this.addFlag)
        {
            this.container.add(new Effect(field,0,16,29))

            // this.container.effectUpdate = (i:integer,o:Effect) => {
            //     o.leftTime = this.leftTime
            //     // o.sprite.setDepth(this.me.container.depth+1)
            // }
        }
    }

    stack(o:FreezeDebuff)
    {
        this.leftTime += o.leftTime
        for (let o of this.container.effects)
        {
            o.leftTime = this.leftTime
        }
    }

    statusModify(status:StatusObject)
    {
        let o = status
        
        o.spd = 0
        if(o.moveAI)
        {
            let me = o.moveAI.me
            let field = o.moveAI.field
            o.moveAI = new DontMove(me,field)
            o.actionAI = new NothingActionAI(me,field,new MersenneTwister())
        }
        return o
    }
}

class PoisonDebuff extends Buff
{
    name = "毒"
    stackable = true
    buffType = BuffType.Debuff
    buffIconIndex = 58

    constructor(me:Character,field:AbstractBattleScene,leftTime:number)
    {
        super(me,field,leftTime)
        this.attachBuff()
    }

    update()
    {
        super.update()
        if(this.time%60 == 0) this.me.hp -= DamageObject.calcPoint(8)
        if(this.time%30 == 0)
        {
            let o = new Effect(this.field,this.me.x-8+Math.random()*16,this.me.y-16,60)
            o.vy = -0.2
            o.leftTime = 60
        }
    }
}

class RegenerateBuff extends Buff
{
    name = "再生"
    stackable = true
    buffType = BuffType.Buff
    buffIconIndex = 67

    constructor(me:Character,field:AbstractBattleScene,leftTime:number)
    {
        super(me,field,leftTime)
        this.attachBuff()
    }

    update()
    {
        super.update()
        if(this.time%60 == 0) this.me.hp += DamageObject.calcPoint(8)
        if(this.time%30 == 0)
        {
            let o = new Effect(this.field,this.me.x-8+Math.random()*16,this.me.y-16,67)
            o.vy = -0.2
            o.leftTime = 60
        }
    }
}

class ReflectBuff extends Buff
{
    name = "反射"
    buffIconIndex = 49
    relativeDepth = -1

    constructor(me:Character,field:AbstractBattleScene,leftTime:number)
    {
        super(me,field,leftTime)
        this.attachBuff()

        this.container.add(new Effect(field,0,0,31))       
        this.container.effectUpdate = (i:integer,o:Effect) => {
            o.sprite.setScale(2.5+Math.sin(this.leftTime/30)/2)
        }
    }

    dmgModify(dmgObj: DamageObject)
    {
        let attacker = dmgObj.bullet.owner
        if (dmgObj.magDmg > 0)
        {
            attacker.scene.sound.play("reflect")
            attacker.hp -= dmgObj.magDmg
        }
        dmgObj.magDmg = 0
        
    }
}

class CoverBuff extends Buff
{
    name = "かばう"
    defender:Character
    buffIconIndex = 57
    relativeDepth = 1

    constructor(me:Character,field:AbstractBattleScene,leftTime:number,defender:Character)
    {
        super(me,field,leftTime)
        this.attachBuff()

        this.defender = defender


        this.container.add(new Effect(field,0,0,37))        
        this.container.effectUpdate = (i:integer,o:Effect) => {
            o.sprite.setScale(2)
            o.y = Math.sin(this.leftTime/50)*4
        }
    }

    update()
    {
        super.update()
        this.drawDotLine()
        if(this.me.hp >= this.defender.hp && this.defender.hp <= 100) this.destroy()
    }

    drawDotLine()
    {
        let g = AbstractBattleScene.g
        g.save()
        g.setDepth(10000)
        g.setAlpha(1/2)
        g.fillStyle(0xffffff)

        let startPos = new Phaser.Math.Vector2(this.me.x,this.me.y)
        let vec = new Phaser.Math.Vector2(this.defender.x-this.me.x,this.defender.y-this.me.y)
        let R = vec.length()
        let r = 0
        let i = 0
        while(r <= R-16)
        {
            r = (this.time/2)%16 + i*16
            vec.setLength(r)
            let pos = startPos.clone().add(vec)
            g.fillCircle(pos.x,pos.y,1.5)
            i++
        }


        g.restore()

    }

    dmgModify(dmgObj: DamageObject)
    {
        let terminalTarget = this.getTerminalTarget()
        if(dmgObj.target != terminalTarget)
        {
            dmgObj.setTarget(terminalTarget)
            dmgObj.targetChangeable = false
        }
    }

    getTerminalTarget()
    {
        let getDefender = (o:Character):Character|null => {
            if(o.hp <= 0) return null
            for(let key in o.buffs)
            {
                let buff = o.buffs[key]
                if(buff instanceof CoverBuff) return buff.defender
                
            }
            return null
        }

        let targets:Character[] = []

        let o:Character = this.me
        let new_o:Character|null = null
        while(true)
        {
            new_o = getDefender(o)
            if (new_o != null)
            {
                if (targets.includes(new_o))
                {
                    return o
                }
                else
                {
                    targets.push(o)
                }
                o = new_o
            }
            else
            {
                return o
            }
        }
    }
}

class SkillLockDebuff extends Buff
{
    name = "アビリティ封印"
    buffType = BuffType.Debuff
    buffIconIndex = 59
    relativeDepth = 1

    constructor(me:Character,field:AbstractBattleScene,leftTime:number)
    {
        super(me,field,leftTime)
        this.attachBuff()

        this.container.add(new Effect(field,-8,-16,61))
        this.container.effectUpdate = (i:integer,o:Effect) => {
            o.y = -16 + Math.sin(-this.time/20)
        }
    }

    statusModify(status:StatusObject)
    {
        let o = status
        if(o.moveAI)
        {
            let me = o.moveAI.me
            let field = o.moveAI.field
            o.actionAI = new OnlyWeaponAttackActionAI(me,field,new MersenneTwister())
        }
        return o
    }
}

class Passive extends Buff
{
    buffType = BuffType.Passive
    
    constructor(me:Character,field:AbstractBattleScene)
    {
        super(me,field,Number.MAX_VALUE)
    }

    update()
    {
        super.update()
        this.leftTime = Number.MAX_VALUE
    }
}

class CrownPassive extends Passive
{
    name = "王冠バフ"
    value = 1.1

    constructor(me:Character,field:AbstractBattleScene,value:number)
    {
        super(me,field)
        this.value = value
    }

    statusModify(status: StatusObject)
    {
        status.mhp *= this.value
        status.mmp *= this.value
        status.atk *= this.value
        status.def *= this.value
        status.spd *= this.value
        status.mag *= this.value
        status.mdef *= this.value
        return status
    }
}

/*================================================================================

TRAIT

=================================================================================*/

class Trait
{
    name = "特性の名前"
    me:Character
    field:AbstractBattleScene

    constructor(character:Character,battleField:AbstractBattleScene,mt?:MersenneTwister)
    {
        this.me = character
        this.field = battleField
    }
}

class StatusAddTrait extends Trait
{
    name = ""
    type:StatusType
    val:integer

    constructor(me:Character,field:AbstractBattleScene,type:StatusType,val:integer)
    {
        super(me,field)
        this.type = type
        this.val = val

        switch(this.type)
        {
            case StatusType.HP: this.name = "HP+"; break;
            case StatusType.MP: this.name = "MP+"; break;
            case StatusType.ATK: this.name = "ATK+"; break;
            case StatusType.DEF: this.name = "DEF+"; break;
            case StatusType.SPD: this.name = "SPD+"; break;
            case StatusType.MAG: this.name = "MAG+"; break;
            case StatusType.MDEF: this.name = "MDEF+"; break;
        }
    }

    start()
    {
        let o = this.me
        switch(this.type)
        {
            case StatusType.HP: o.mhp += this.val; break;
            case StatusType.MP: o.mmp += this.val; break;
            case StatusType.ATK: o.atk += this.val; break;
            case StatusType.DEF: o.def += this.val; break;
            case StatusType.SPD: o.spd += this.val; break;
            case StatusType.MAG: o.mag += this.val; break;
            case StatusType.MDEF: o.mdef += this.val; break;
        }
    }

    update()
    {

    }
}

class SuddenSpiritBuffTrait extends Trait
{
    name = "いきなり精霊の加護"
    
    start()
    {
        new SpiritBuff(this.me,this.field)
    }
}

/*================================================================================

BULLET ENCHANT

=================================================================================*/
class BulletEnchant
{

    enchant(dmgObj:DamageObject)
    {

    }
}

class HPAbsorbEnchant extends BulletEnchant
{
    rate:number = 1

    constructor(rate:number)
    {
        super()
        this.rate = rate
    }

    enchant(dmgObj:DamageObject)
    {
        dmgObj.bullet.owner.hp += Math.floor((dmgObj.phyDmg+dmgObj.magDmg)*this.rate)
    }
}

class MPAbsorbEnchant extends BulletEnchant
{
    rate:number = 1

    constructor(rate:number)
    {
        super()
        this.rate = rate
    }

    enchant(dmgObj:DamageObject)
    {
        dmgObj.bullet.owner.mp += Math.floor((dmgObj.mpDmg)*this.rate)
    }
}

class FreezeEnchant extends BulletEnchant
{
    time:number = 0

    constructor(time:number)
    {
        super()
        this.time = time
    }

    enchant(dmgObj: DamageObject)
    {
        new FreezeDebuff(dmgObj.target,dmgObj.target.scene,this.time)
    }
}

class PoisonEnchant extends BulletEnchant
{
    time:number = 0

    constructor(time:number)
    {
        super()
        this.time = time
    }

    enchant(dmgObj: DamageObject)
    {
        new PoisonDebuff(dmgObj.target,dmgObj.target.scene,this.time)
    }
}



/*================================================================================

WEAPON

=================================================================================*/

class Weapon
{
    // me:Character
    // sprite:Phaser.GameObjects.Sprite
    atk = 0
    mag = 0
    range = 0
    hp = 0
    mp = 0
    def = 0
    spd = 0
    mdef = 0

    weaponType = 0
    index = 0

    bulletSpread = 0

    enchants:BulletEnchant[] = []

    constructor(weaponType:WeaponType,index:number)
    {
        // this.me = character
        // this.sprite = this.field.add.sprite(0,0,"weapons",getWeaponSpriteFrame(weaponType,index))
        this.weaponType = weaponType
        this.index = index
        this.setStatus()
    }

    setStatus()
    {
        let e:BulletEnchant[] = []
        let o:integer[] = []

        const mr:integer = 35
        if (this.weaponType == WeaponType.SWORD)
        {
            switch (this.index)
            {
                case 0: o = [40,0,mr,0,0,0,0,0]; break;
                case 1: o = [30,0,mr,30,0,30,-10,0]; break;
                case 2: o = [60,30,mr,0,30,0,0,0]; break;
                case 3: o = [10,0,mr,50,0,0,-40,0];e = [new HPAbsorbEnchant(0.2)]; break;
                case 4: o = [10,30,mr,0,30,0,0,0];e = [new FreezeEnchant(5)]; break;
                case 5: o = [10,30,mr,0,30,0,0,0];e = [new PoisonEnchant(30)]; break;
            }
        }
        else if(this.weaponType == WeaponType.BOW)
        {
            this.bulletSpread = Math.PI/6
            switch (this.index)
            {
                case 0: o = [40,0,200,0,0,0,0,0]; break;
                case 1: o = [80,0,160,0,0,0,-40,0]; break;
                case 2: o = [10,0,240,0,0,0,0,0]; break;
                case 3: o = [10,0,160,-50,0,0,-40,0];e = [new HPAbsorbEnchant(0.2)]; break;
                case 4: o = [10,30,160,0,30,0,0,0];e = [new FreezeEnchant(5)]; break;
                case 5: o = [10,30,160,0,30,0,0,0];e = [new PoisonEnchant(30)]; break;
            }
        }
        else if(this.weaponType == WeaponType.STAFF)
        {
            switch (this.index)
            {
                case 0: o = [-100,100,mr,0,30,0,-100,30]; break;
                case 1: o = [40,50,mr,0,30,0,-40,30]; break;
                case 2: o = [-100,150,mr,0,60,0,-100,60]; break;
                case 3: o = [-100,150,mr,0,60,0,-100,60];e = [new FreezeEnchant(5)]; break;
                case 4: o = [-100,150,mr,0,60,0,-100,60];e = [new PoisonEnchant(30)]; break;
            }
        }
        else if(this.weaponType == WeaponType.GUN)
        {
            this.bulletSpread = Math.PI/24
            switch (this.index)
            {
                case 0: o = [40,-100,160,0,-100,0,-100,0]; break;
                case 1: o = [300,-100,300,0,-100,0,-250,0];this.bulletSpread = 0; break;
                case 2: o = [10,-100,200,0,-100,0,100,0];this.bulletSpread = Math.PI/3; break;
            }
        }
        
        this.enchants = e
        if(o.length != 8) console.error("ステータスの配列が8になっていません！")
        this.atk = o[0]
        this.mag = o[1]
        this.range = o[2]
        this.hp = o[3]
        this.mp = o[4]
        this.def = o[5]
        this.spd = o[6]
        this.mdef = o[7]

    }

    getSpriteFrame()
    {
        return getWeaponSpriteFrame(this.weaponType,this.index)
    }
}





class Bullet
{
    owner:Character
    field:AbstractBattleScene
    sprite:Phaser.Types.Physics.Arcade.SpriteWithDynamicBody
    leftTime = 1000
    hitSound = "hit"
    delayTime = 0
    delayFlag = false
    enchants:BulletEnchant[] = []

    get x()
    {
        return this.sprite.x
    }
    set x(val:number)
    {
        this.sprite.x = val
    }
    get y()
    {
        return this.sprite.y
    }
    set y(val:number)
    {
        this.sprite.y = val
    }

    _pierceCount = 1
    set pierceCount(val:integer)
    {
        this._pierceCount = val
        if (this._pierceCount <= 0)
        {
            this.die()
        }
    }
    get pierceCount()
    {
        return this._pierceCount
    }
    atk = 0
    mag = 0
    mpAtk = 0
    // 多分弾のdefとmdefはほぼ使わないでしょう。
    def = 0
    mdef = 0

    autoRotAlongVelocity = true

    constructor(owner:Character,field:AbstractBattleScene,frame:integer,atkPow:integer,magPow:integer,spriteName:string="sprites")
    {
        this.owner = owner
        this.field = field
        this.sprite = field.physics.add.sprite(this.owner.x,this.owner.y,spriteName,frame)
        this.sprite.setDepth(10000)

        this.atk = atkPow
        this.mag = magPow

        this.sprite.setSize(8,8)
        this.sprite.setScale(2)

        field.setLayer(this)
    }

    update()
    {
        if (!this.sprite)return
        if (this.autoRotAlongVelocity)
        {
            this.sprite.rotation = this.sprite.body.velocity.angle()
        }
        if (this.delayFlag)
        {
            this.delayTime--
            if(this.delayTime <= 0)
            {
                this.sprite.enableBody()
                this.sprite.setAlpha(1)
                this.delayFlag = false
            }
        }
        this.leftTime--
        if(this.leftTime <= 0)
        {
            this.die()
        }
    }

    shotTo(x:integer,y:integer,speed:number)
    {
        this.sprite.setPosition(this.owner.x+this.owner.getFlipX(8),this.owner.y-4)
        
        let vec = new Phaser.Math.Vector2(x-this.sprite.x,y-this.sprite.y)
        vec = vec.setLength(speed)
        this.sprite.setVelocity(vec.x,vec.y)
    }

    die()
    {
        this.sprite.destroy()
    }

    setDelay(delay:number)
    {
        this.delayFlag = true
        this.sprite.body.enable = false
        this.delayTime = delay
        this.sprite.setAlpha(0)
    }
}

class WeaponBullet extends Bullet
{
    time = 0
    rotSpd = 0

    constructor(owner:Character,field:AbstractBattleScene)
    {
        let st = owner.getModifiedStatus()
        let weapon = owner.weapon
        super(owner,field,weapon.getSpriteFrame(),st.atk,0,"weapons")
        this.rotSpd = Math.max(200/20000,st.spd/20000)
        this.sprite.setOrigin(1/2,1/2)
        this.sprite.setScale(2,2)

        this.sprite.body.setSize(8,24)
        
        this.leftTime = 3/(this.rotSpd*2)
        this.setPosition()

        this.owner.weaponSprite.setVisible(false)
        
        
    }

    update()
    {
        super.update()
        this.setPosition()
        this.time++
    }

    die()
    {
        // this.owner.weaponSprite.setVisible(true)
        super.die()
    }

    setPosition()
    {
        let scaleX = this.owner.container.scaleX > 0 ? 1 : -1
        this.sprite.x = this.owner.x + (8 + 16 * Math.cos(Math.PI/2-this.time*this.rotSpd)) * scaleX
        this.sprite.y = this.owner.y + 12 - 16 * Math.sin(Math.PI/2-this.time*this.rotSpd)
        this.sprite.rotation = this.time*this.rotSpd * scaleX 
        this.sprite.setScale(2*scaleX,2)
        if(this.sprite.body)
        {
            if (scaleX > 0)
            { 
                this.sprite.body.setOffset(4,-4)
            }
            else
            {
                this.sprite.body.setOffset(12,-4)
            }
        }
    }
}

class FloatingBubble extends Bullet
{
    autoRotAlongVelocity = false
    extraAx = 0
    extraAy = 0

    constructor(owner:Character,field:AbstractBattleScene)
    {
        let st = owner.getModifiedStatus()
        let bMag = Math.floor(st.mag/5)
        super(owner,field,20,0,bMag)
        this.leftTime = 360 + Math.random()*120

        this.extraAx = (-50+Math.random()*100)/1000
        this.extraAy = (-50+Math.random()*100)/1000
    }

    update()
    {
        super.update()
        if(!this.sprite.body)return
        
        this.sprite.setAcceleration(-this.sprite.body.velocity.x/100, -this.sprite.body.velocity.y/100)
        this.sprite.body.setAcceleration(this.sprite.body.acceleration.x+this.extraAx,this.sprite.body.acceleration.y+this.extraAy)
    }

    die()
    {
        {
            let o = new Effect(this.field,this.x,this.y,30)
            o.leftTime = 2
            this.field.sound.play("bubble")
        }
        super.die()
    }

}

class Effect
{
    name:string = "UUIDの為に使う"
    field:AbstractBattleScene
    leftTime = 60
    sprite:Phaser.GameObjects.Sprite
    get x()
    {
        return this.sprite.x
    }
    set x(val:number)
    {
        this.sprite.x = val
    }
    get y()
    {
        return this.sprite.y
    }
    set y(val:number)
    {
        this.sprite.y = val
    }
    vx = 0
    vy = 0
    ax = 0
    ay = 0
    delayTime = 0
    delayFlag = false
    originalX = 0
    originalY = 0
    // alphaFunc?:Function //透明度の変化速度
    updateFunc:(self:Effect,t:integer) => void = (self:Effect,t:integer) => {}

    time = 0

    constructor(field:AbstractBattleScene,x:number,y:number,frame:integer,animName?:string)
    {
        this.field = field
        this.sprite = field.add.sprite(x,y,"sprites",frame)
        this.sprite.setDepth(10000)
        this.sprite.setScale(2)
        this.originalX = x
        this.originalY = y

        if(animName)
        {
            this.sprite.anims.play(animName)
        }

        // field.effects.push(this)
        field.addEffect(this)
    }

    update()
    {
        if(!this.sprite)return
        if (!this.delayFlag)
        {
            this.sprite.x += this.vx
            this.sprite.y += this.vy
            this.vx += this.ax
            this.vy += this.ay
        }
        if (this.delayFlag)
        {
            this.delayTime--
            if(this.delayTime <= 0)
            {
                this.sprite.setAlpha(1)
                this.delayFlag = false
            }
        }
        // if(this.alphaFunc)
        // {
        //     this.sprite.setAlpha(this.alphaFunc(this.time))
        // }
        if(this.updateFunc)
        {
            this.updateFunc(this,this.time)
        }
        this.leftTime--
        if(this.leftTime <= 0)
        {
            this.destroy()
        }
        this.time++
    }

    destroy()
    {
        this.sprite.destroy()
        this.field.removeEffect(this)
    }

    setDelay(delay:number)
    {
        this.delayFlag = true
        this.delayTime = delay
        this.sprite.setAlpha(0)
    }
}



enum NumberType
{
    DAMAGE,
    HPHEAL,
    MPHEAL,
    MPDAMAGE,
}

class NumberUI
{
    name:string = "UUIDの為に使う"
    field:AbstractBattleScene
    leftTime = 60
    container: Phaser.GameObjects.Container
    sprites:Phaser.GameObjects.Sprite[] = []
    vy = -0.8
    ay = 0.02
    constructor(field:AbstractBattleScene,x:number,y:number,num:integer,type:NumberType=NumberType.DAMAGE)
    {
        this.field = field
        let numStr = num.toString()
        this.container = field.add.container(x,y)
        for (let i = 0; i < numStr.length; i++)
        {
            let o = field.add.sprite(12-12*i,0,"numbers",type*10+Number.parseInt(numStr[numStr.length-1-i]))
            o.setScale(2)
            this.sprites.push(o)
            this.container.add(o)
        }
        this.container.setDepth(20000)
        // field.effects.push(this)
        field.addEffect(this)
    }

    update()
    {
        this.container.y += this.vy
        this.vy += this.ay
        this.leftTime--
        if(this.leftTime <= 0)
        {
            this.die()
        }
    }

    die()
    {
        for (let o of this.sprites)
        {
            o.destroy()
        }
        this.field.removeEffect(this)
    }
}

class ThunderCloud extends Effect
{
    container:Phaser.GameObjects.Container
    clouds:Effect[] = []
    deltas:number[] = []
    target:Character
    owner:Character
    constructor(owner:Character,field:AbstractBattleScene,target:Character)
    {
        let x = target.x
        let y = target.y - 100
        super(field,x,y,5)
        this.owner = owner
        this.field = field
        this.target = target
        this.sprite.setAlpha(0)
        this.container = field.add.container(0,0)
        this.clouds = []
        this.leftTime = 2400
        for (let i = 0; i < 7; i++)
        {
            let frame = 5+Math.floor(Math.random()*2)
            let e = new Effect(field,-32+Math.random()*64,Math.random()*16,frame)
            e.leftTime = 2200 + Math.random()*60 + (frame==6?60:0)
            this.container.add(e.sprite)
            e.setDelay(Math.random()*120)
            this.clouds.push(e)
            this.deltas.push(Math.random()*10)
        }
        this.container.setPosition(x,y)
        this.container.setDepth(this.sprite.depth)
    }

    update()
    {
        super.update()

        let vec = new Phaser.Math.Vector2(this.target.x-this.container.x,this.target.y-100-this.container.y)
        if (vec.length() <= 0.4)
        {
            this.container.setPosition(this.target.x,this.target.y-100)
        }
        else
        {
            vec = vec.normalize().scale(0.4)
            this.container.setPosition(this.container.x+vec.x,this.container.y+vec.y)
        }
        for(let i = 0; i < this.clouds.length;i++)
        {
            let o = this.clouds[i]
            this.clouds[i].sprite.setPosition(o.originalX+4*Math.cos(this.deltas[i]+this.leftTime/30),o.originalY+4*Math.sin(this.deltas[i]+this.leftTime/30))
        }

        if(this.leftTime > 0)
        {
            if(Math.random()*10000 <= 10000/200)
            {
                this.field.sound.play("thunder")
                let st = this.owner.getModifiedStatus()
                let bMag = Math.floor(st.mag/2)
                let o = new Bullet(this.owner,this.field,7,0,bMag)
                o.sprite.setVelocity(0,300)
                o.sprite.setPosition(this.container.x,this.container.y)
                o.leftTime = 50
                o.autoRotAlongVelocity = false
            }
        }
        
    }
}

class BeamHead extends Effect
{
    owner: Character
    rot: number
    constructor(owner:Character,field:AbstractBattleScene,target:Character)
    {
        super(field,owner.x,owner.y,10)

        let vec = new Phaser.Math.Vector2(target.x-owner.x,target.y-owner.y).normalize().scale(5)
        this.vx = vec.x
        this.vy = vec.y
        this.rot = vec.angle()
        this.sprite.setRotation(this.rot)

        this.owner = owner
        this.field = field
        this.leftTime = 100
        this.sprite.setDepth(this.sprite.depth+1)
        

    }

    update()
    {
        super.update()

        if (this.leftTime%2 == 0)
        {
            let st = this.owner.getModifiedStatus()
            let bMag = Math.floor(st.mag/2)
            let o = new Bullet(this.owner,this.field,9,0,bMag)
            o.sprite.setPosition(this.sprite.x,this.sprite.y)
            o.sprite.setRotation(this.rot)
            o.autoRotAlongVelocity = false
            o.leftTime = 40
            o.hitSound = "beam1"
        }
    }
}

class ContainerWrapper extends Phaser.GameObjects.Container
{
}

class EffectContainer
{
    container:Phaser.GameObjects.Container
    effects:Effect[] = []
    x:number = 0
    y:number = 0
    effectUpdate:Function

    constructor(field:AbstractBattleScene,x:number=0,y:number=0)
    {
        this.x = x
        this.y = y
        this.container = field.add.container()
        this.effectUpdate = (i:integer,o:Effect) => {}
    }

    update()
    {
        for (let i = 0; i < this.effects.length; i++)
        {
            let o = this.effects[i]
            o.leftTime = 10000
            this.effectUpdate(i,o)
            
        }
    }

    add(o:Effect)
    {
        this.effects.push(o)
        this.container.add(o.sprite)
        o.leftTime = 10000
    }

    destroy()
    {
        for (let o of this.effects)
        {
            o.destroy()
        }
    }

    
}

class DamageObject
{
    bullet:Bullet
    originalTarget:Character
    target:Character

    phyDmg:integer = 0
    magDmg:integer = 0
    mpDmg:integer = 0

    isShowHPDmg:boolean = true

    recalcFlag = false
    // recalcCount = 0
    targetChangeable = true

    constructor(bullet:Bullet,target:Character)
    {
        this.bullet = bullet
        this.originalTarget = target
        this.target = target
    }

    setCalcDmg()
    {
        this.recalcFlag = false

        let o1 = this.bullet
        let o2 = this.target
        
        let st2 = o2.getModifiedStatus()

        this.phyDmg = DamageObject.calcDmgPoint(o1.atk,st2.def)
        this.magDmg = DamageObject.calcDmgPoint(o1.mag,st2.mdef)
        this.mpDmg = DamageObject.calcDmgPoint(o1.mpAtk,st2.mdef)
        
        //バフの影響
        for (let key in o2.buffs)
        {
            let buff = o2.buffs[key]
            buff.dmgModify(this)
        }

        //弾のエンチャントの影響
        for (let e of o1.enchants)
        { 
            e.enchant(this)
        }
        
        if(this.recalcFlag) this.setCalcDmg()
    }

    static calcDmgPoint(atkPoint:integer,defPoint:integer):integer
    {
        let dmg = DamageObject.calcPoint(atkPoint)
        // if(dmg>0)console.log(`dmg:${dmg},${atkPoint},${defPoint}`)
        return Math.max(0, Math.floor(dmg/(1+defPoint/100)))
        // return Math.max(1, Math.floor(dmg - defPoint))
    }

    static calcPoint(val:integer):integer
    {
        return Math.floor(val*(Math.random()*0.2+0.9))
    }

    setTarget(target:Character)
    {
        if(!this.targetChangeable)return
        // if(this.recalcCount >= 1)return
        this.target = target
        // this.recalcCount++
        this.recalcFlag = true
    }

    dmg()
    {
        let hpDmg = this.phyDmg + this.magDmg

        if(this.mpDmg > 0 && hpDmg == 0) this.isShowHPDmg = false

        if(this.isShowHPDmg)
        {
            this.target.hp -= hpDmg
        }
        else
        {
            this.target.setHP(this.target.hp-hpDmg,false)
        }
        if(this.mpDmg > 0)
        {
            this.target.mp -= this.mpDmg
        }
        AbstractBattleScene.scene.sound.play(this.bullet.hitSound)

        this.bullet.pierceCount--
    }


}

class SaveDataManager
{
    static save2()
    {
        let o = ""
        let num = 0
        for(let i = 0; i < ChallengePage.challengeCompleteList.length; i++)
        {
            num *= 2
            let flag = ChallengePage.challengeCompleteList[i]
            if (flag) num += 1
            if (i % 4 == 3)
            {
                o += num.toString(16)
                num = 0
            }
        }
        SaveDataManager.saveCookieTemplate("clears",o)
        SaveDataManager.saveCookieTemplate("rank",RankBattlePage.rank.toString(16))
        SaveDataManager.saveCookieTemplate("maxRank",RankBattlePage.maxRank.toString(16))
        SaveDataManager.saveCookieTemplate("memo",encodeURIComponent(MemoPage.memoArea.value))
    }

    static load2()
    {
        SaveDataManager.loadCookieTemplate("clears",(o)=>{
            for(let i = 0; i < o.length; i++)
            {
                let c = o[i]
                let num = parseInt(c,16)
                for(let j = 0; j < 4; j++)
                {
                    if (num & 2 ** (3-j))
                    {
                        ChallengePage.challengeComplete(i*4+j+1)
                    }
                }
            }
        })
        SaveDataManager.loadCookieTemplate("rank",(o)=>{
            RankBattlePage.rank = parseInt(o,16)
        })
        SaveDataManager.loadCookieTemplate("maxRank",(o)=>{
            RankBattlePage.maxRank = parseInt(o,16)
        })
        SaveDataManager.loadCookieTemplate("memo",(o)=>{
            MemoPage.memoArea.value = decodeURIComponent(o)
        })
    }

    static encodeMemo(o:string)
    {
        o = o.replace(/\//g,"${／}")
        o = o.replace(/\n/g,"${＼n}")
        return o
    }

    static decodeMemo(o:string)
    {
        o = o.replace(/\$\{／\}/g,"/")
        o = o.replace(/\$\{＼n\}/g,"\n")
        return o
    }

    static getCookie():string
    {
        return document.cookie
    }

    static setCookie(text:string)
    {
        document.cookie = text
    }
    
    static loadCookieTemplate(key:string,func:(cookie:string)=>void)
    {
        let o = Cookies.get(key)
        if(o)func(o)
    }

    static saveCookieTemplate(key:string,value:string)
    {
        Cookies.set(key,value,{expires:36500})
    }
}




class SearchTool
{
    static search(_name:string)
    {
        for(let i = 0; i < 1000; i++)
        {
            let name = _name+i
            let o = new Character(AbstractBattleScene.scene,name)
            SearchTool.showInfo(o)
            o.destroy()
        }
    }

    static search2(prefix:string)
    {
        for(let n of NAMES)
        {
            let name = prefix + n
            let o = new Character(AbstractBattleScene.scene,name)
            SearchTool.showInfo(o)
            o.destroy()
        }
    }

    static search3(prefix:string,rank:integer)
    {
        for(let n of NAMES)
        {
            let name = prefix + n
            let o = new Character(AbstractBattleScene.scene,name)
            let sum = Math.floor(o.mhp/4) + o.mmp + o.atk + o.def + o.mag + o.mdef + o.spd
            if(sum>=rank-200&&sum<=rank+200)
            {
                console.log(`${name},sum:${sum}`)
            }
        }
    }

    static showInfo(o:Character)
    {
        let name = o.name
        let sum = Math.floor(o.mhp/4) + o.mmp + o.atk + o.def + o.mag + o.mdef + o.spd
        if(o.spd >= 500 && (o.atk >= 500 || o.mag >= 500) && o.hp >= 1500 && sum >= 3500)
        {
            console.log(`${name},spd:${o.spd},sum:${sum}`)
        }
        if(sum >= 4000)
        {
            console.log(`${name},spd:${o.spd},sum:${sum}`)
        }
        if(o.hp >= 3000 && o.def >= 700)
        {
            console.log(`${name},hp:${o.hp},def:${o.def},spd:${o.spd},sum:${sum}`)
        }
        if(SearchTool.hasAction(o,["ダウン","ブースト","ヒール"]) && SearchTool.hasAction(o,["瞑想"]) && o.spd >= 500 && o.hp >= 2000 && o.def >= 400)
        {

            console.log(`${name},spd:${o.spd},sum:${sum},${o.actions[1].name},${o.actions[2].name},${o.actions[3].name}`)
        }
        if(SearchTool.hasAction(o,["仲間を呼ぶ"]) && SearchTool.hasAction(o,["瞑想"]) && o.mp >= 700)
        {
            console.log(`${name},仲間を呼ぶ`)
        }
        if(SearchTool.hasAction(o,["瞑想","マナドレイン"]) && o.mmp+o.mag+o.mdef >= 2000)
        {
            console.log(`$${name},魔導士,sum:${sum}`)
        }
        if(SearchTool.hasAction(o,["ヒール"])) console.log(`${name},ヒール`)
    }

    static hasAction(o:Character,actionNames:string[])
    {
        for(let a of o.actions)
        {
            for(let actionName of actionNames)
            {
                if (a.name.includes(actionName))
                {
                    return true
                }
            }
        }
        return false
    }
}

class CharacterValueCalculator
{
    static calc(name:string)
    {
        let mt = new MersenneTwister()
        let unicodes = []
        for (let i = 0; i < name.length; i++)
        {
            unicodes.push(name.charCodeAt(i))
        }
        var nums = []
        for (let i = 0; i < unicodes.length; i++)
        {
            mt.seed(unicodes[i])
            nums.push(mt.int())
        }

        // ここで、完全な名前による乱数が完成する。
        mt.seedArray(unicodes)
        
        let mhp:integer = Math.max(1,mt.int()%800+mt.int()%800+mt.int()%800+mt.int()%800+mt.int()%800)
        let mmp:integer = Math.max(1,mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200)
        let atk:integer = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200
        let def:integer = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200
        let spd:integer = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200
        let mag:integer = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200
        let mdef:integer = mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200+mt.int()%200

        // 11回なのは、色(5)＋アバター(5)＋MoveAIのせい
        for (let i = 0; i < 11; i++) mt.int()
        if (mt.int()%3 == 2)
        {
            for(let i = 0; i < 4; i++) mt.int()
        }

        let weaponType = mt.int()%4 as WeaponType
        let weaponIndex = 0
        if (weaponType == WeaponType.SWORD) weaponIndex = mt.int()%6
        else if (weaponType == WeaponType.BOW) weaponIndex = mt.int()%6
        else if (weaponType == WeaponType.STAFF) weaponIndex = mt.int()%5
        else if (weaponType == WeaponType.GUN) weaponIndex = mt.int()%3
        let weapon = new Weapon(weaponType,weaponIndex)
        
        atk += weapon.atk
        mag += weapon.mag
        def += weapon.def
        spd += weapon.spd
        mhp += weapon.hp
        mmp += weapon.mp
        mdef += weapon.mdef
        
        mhp = Math.max(1, mhp)
        mmp = Math.max(0, mmp)
        atk = Math.max(0, atk)
        def = Math.max(0, def)
        spd = Math.max(0, spd)
        mag = Math.max(0, mag)
        mdef = Math.max(0,mdef)

        return Math.floor(mhp/4)+mmp+atk+def+spd+mag+mdef
    }
}

const PREFICES:string[] = [
    "闘士","ウォーリア","拳闘士","格闘家","打撃者","ファイター","ボクサー","ストライカー","剣闘士","グラディエーター","ムルミロ","剣士","フェンサー","ソードマン","ソードファイター","セイバー","剣聖","ソードマスター","侍","サムライ","浪人","ローニン","兵士","ソルジャー","歩兵","ロデレロ","エスパダチン","軽装歩兵","重装歩兵","ホプライト","近衛兵","ロイヤルガード","騎士","ナイト","軽騎士","ソシアルナイト","重騎士","アーマーナイト","クラッシャー","女騎士","姫騎士","暗黒騎士","闇騎士","ダークナイト","黒騎士","ブラックナイト","シャドウナイト","竜騎士","ドラグナー","ドラゴンナイト","天馬騎士","ペガサスナイト","聖騎士","パラディン","騎士団長","ナイトリーダー","騎兵","騎乗兵","トローパー","ソシアルナイト","カタフラクト","ライダー","戦士","ファイター","女戦士","アマゾネス","アマゾン","狂戦士","バーサーカー","ベルセルク","重戦士","アーマー","ファランクス","蛮族","バーバリアン","守護者","防衛者","ガーディアン","ガード","ディフェンダー","盾使い","シールダー","武闘家","武道家","グラップラー","傭兵","ハイランダー","マーシナリー","槍使い","ランサー","バトルマスター","ゴッドハンド","死刑執行人","討伐者","スレイヤー","弓使い","アーチャー","エルフ","銃士","射手","ガンナー","ガンマン","ガンスリンガー","狙撃手","スナイパー","弓騎兵","ボウナイト","ホースメン","狩人","ハンター","砲手","シューター","せんしゃ","竜騎兵","ドラグーン","爆撃兵","ボマー","暗殺者","アサシン","アサッシン","盗賊","シーフ","海賊","コルセア","パイレーツ","山賊","バンデット","怪盗","トリックスター","ごろつき","悪党","野党","ローグ","密偵","スカウト","諜報員","スパイ","忍者","ニンジャ","女忍者","くノ一","工作員","サッパー","軍師","タクティシャン","侍僧","アコライト","聖職者","クレリック","教皇","ポープ","枢機卿","カーディナル","司教","ビショップ","大司教","アークビショップ","司祭","神父","神官","プリースト","プリーステス","大司祭","高位神官","ハイプリースト","ハイプリーステス","暗黒神官","闇神官","ダークプリースト","牧師","プリーチャー","僧侶","女僧侶","暗黒僧侶","闇僧侶","高僧","僧兵","神官戦士","神殿騎士","テンプル騎士","テンプルナイト","聖騎士","ホーリーナイト","ホワイトナイト","パラディン","神聖騎士","ディバインナイト","ゴーディアン","聖戦士","クルセイダー","異端審問官","尋問者","インクイジター","代行者","呪術師","巫覡","巫女","神子","祈祷師","シャーマン","ドルイド","ウァテス","バルド","修道僧","修道士","モンク","モンク僧","修道女","シスター","カリフ","預言者","プロフェット","裁定者","ルーラー","隠者","仙人","ハーミット","魔女","ウィッチ","ウォーロック","リトルウィッチ","魔法使い","ウィザード","メイジ","マジシャン","大魔法使い","大魔導","ハイウィザード","アークメイジ","マジカルスター","魔導師","魔道師","魔導士","魔道士","魔術師","魔術士","キャスター","ソーサラー","ソーサレス","コンジャラー","ダイバー","黒魔術師","黒魔導師","黒魔道士","ダークマージ","白魔術師","白魔導師","白魔道士","付与魔術師","エンチャンター","祓魔師","エクソシスト","退魔師","陰陽師","死霊","リッチ","キングリッチ","占い師","占星術師","スターシーカー","予言者","風水士","風水師","道士","施療師","ヒーラー","超能力者","エスパー","召喚師","召喚士","サマナー","サモナー","シャーマン","精霊使い","エレメンタラー","死霊魔術師","ネクロマンサー","人形師","獣使い","ビーストテイマー","猛獣使い","調教師","竜使い","ドラゴンテイマー","ドラゴンマスター","魔物使い","魔獣使い","ビーストマスター","鳥使い","蟲使い","虫使い","商人","マーチャント","踊り子","ダンサー","アイドル","吟遊詩人","バード","トルバドール","ミンストレル","道化師","アルルカン","クラウン","大道芸人","旅芸人","ヴァグランツ","売春婦","娼婦","学者","スカラー","魔道学者","冒険者","バックパッカー","アドベンチャラー","賭博師","ギャンブラー","運び屋","遊び人","錬金術師","アルケミスト","薬師","科学者","職人","刀鍛冶","刀匠","鍛冶屋","ブラックスミス","木工師","大工","カーペンター","裁縫師","彫金師","ゴールドスミス","釣り師","漁師","発明家","工芸家","クラフター","農民","農夫","ファーマー","炭鉱夫","木こり","調理師","チューシ","りょうりにん","魔法戦士","魔戦士","魔術戦士","ルーンナイト","マジックナイト","銃剣士","ガンブレイダー","賢者","女賢者","セイジ","セージ","王","貴族","ノーブル","男爵","バロン","バロネス","子爵","バイカウント","伯爵","カウント","侯爵","フェルスト","公爵","デューク","王","国王","キング","女王","クイーン","王子","プリンス","王女","プリンセス","君主","ロード","皇帝","エンペラー","カイザー","帝","ミカド","大名","ダイミョー","将軍","ジェネラル","ショーグン","一般人","野伏","レンジャー","市民","シビリアン","村人","牧童","乞食","べガー","勇者","女勇者","英雄","ヒーロー","ブレイブ","ブレイバー","魔王","大魔王",
    "自営業","農家","ファーマー","木こり","花屋","宿屋","なんでも屋","武器屋","防具屋","魔法屋","アイテム屋","アクセサリー屋","洋服屋","料理店","ケーキ屋","パン屋","喫茶店","酒場","ペットショップ","水商売","芸能","マスコミ","カメラマン","アイドル","歌手","職人","絵描き","漫画家","大工","庭師","炭坑夫","舞踊家","武道家","バレリーナ","鍛冶師","ブラックスミス","機械","ハッカー","エンジニア","プログラマー","公務員","教師","家庭教師","校長","政治家","執事","秘書","保母","保父","村人","主婦","無職","学生","牧童","墓守","占い師","メイド","侍女","サーカス団員","プロタゴニスト","ヒーロー","アンチヒーロー","ダークヒーロー","敵対者","エネミー","アンタゴニスト","モブ","一般市民","村人","ビレジャー","労働者","ワーカー","奴隷","スレイブ","難民","レフュジー","貧民","プアー","乞食","ベガー","放浪者","ジプシー","領主","フューダルロード","貴族","ノーブル","富豪","ブルジョワ","王族","ロイヤル","貴下","サー","女士爵","ディム","貴婦人","マダム","男爵","バロン","子爵","ヴァイカウント","伯爵","アール","カウント","侯爵","マルキス","公爵","デューク","大公","アーチデューク","グランドデューク","閣下","エクセレンシー","君主","ロード","王女","プリンセス","王子","プリンス","女王","クイーン","王","キング","皇帝","天皇","エンペラー","道化師","クラウン","旅人","トラベラー","用心棒","バウンサー","ボディーガード","傭兵","マーシナリー","討伐者","スレイヤー","勇者","ブレイバー","冒険者","初心者","ノービス","冒険家","アドベンチャラー","探検家","探求者","エクスプローラー","見習い剣士","ルーキー","戦士","ファイター","女戦士","アマゾネス","軽戦士","フェンサー","重戦士","ヘビィファイター","槍騎兵","ランサー","守護者","ガーディアン","剣士","ソードマン","剣聖","ソードマスター","狂戦士","バーサーカー","戦闘狂","ベルセルク","闘士","ウォーリア","剣闘士","グラディエーター","剣闘王","チャンピオン","魔法戦士","精霊戦士","乗る者","ライダー","騎士","ナイト","騎兵","トルーパー","キャバルリー","重騎兵","軽騎兵","驃騎兵","ハサー","弓騎兵","聖騎士","パラディン","聖堂騎士","テンプルナイト","神聖騎士","ディバンナイト","ゴーディアン","聖戦士","十字軍","クルセイダー","竜騎兵","ドラグーン","天馬騎士","ペガサスナイト","魔法騎士","マジカルナイト","精霊騎士","決闘者","デュエリスト","兵士","ソルジャー","候補生","カデット","士官候補生","エリート","騎士","ナイト","将軍","ジェネラル","軍師","ウォーロード","訓練兵","トレーニー","専業軍人","アーミー","門番","ゲートキーパー","衛兵","守衛","ガード","近衛兵","斥候","偵察","スカウト","防衛者","ディフェンダー","傭兵","マーシナリー","工作兵","ワーカー","輸送兵","トランスポーター","衛生兵","軍医","医官","前線司令官","マーシャル","司令官","指揮官","コマンダー","総司令官","最高司令官","最高指揮官","コマンダー","イン","チーフ","参謀長","幕僚長","砲兵","ガンナー","銃使い","ガンスリンガー","弓使い","クロスボウマン","射手","アーチャー","射手","シューター","魔砲使い","猟師","狩人","ハンター","狙撃手","スナイパー","爆弾兵","ボマー","罠師","トラップマスター","山賊","バンディット","海賊","パイレーツ","蛮族","バーバリアン","戦士","ファイター","モンク","武闘家","拳士","拳闘家","拳闘士","ピュージリスト","パグ","格闘家","格闘士","グラップラー","打撃者","ストライカー","拳聖","ゴッドハンド","拳帝","強大な支配者","マイティルーラー","拳闘王","チャンプ","チャンピオン","ボクサー","レスラー","空手家","カラテカ","喧嘩師","ブロウラー","素手喧嘩","ステゴロ","破壊者","デストロイヤー","気功師","魔女見習い","隠者","ハーミット","魔法使い","マジックユーザー","魔法少女","マジカルウィッチ","魔女","ウィッチ","リトルウィッチ","魔法使い♂","ウォーロック","魔術師","ウィザード","メイジ","魔導士","ソーサラー","ソーサレス","ダイバー","賢者","セージ","ワイズマン","占い師","予言者","先見者","プロフェット","シアー","呪文使い","スペルキャスト","召喚術士","サモナー","精霊術士","エレメンタリスト","錬金術師","アルケミスト","占星術士","アストロジスト","風水師","ドルイド","呪殺師","カースメーカー","除霊師","ゴーストバスター","祓魔師","エクソシスト","呪術師","シャーマン","操霊魔導士","コンジャラー","死霊術士","ネクロマンサー","呪言使い","ルーンマスター","囁く者","ウィスパー","付与魔法使い","エンチャンター","符術使い","ルーン術師","ルーンキャスター","変幻魔導士","超能力者","エスパー","サイキッカー","思念透視者","サイコメトリスト","奇術師","マジシャン","幻術師","心理術師","メンタリスト","ヒーラー","修行僧","モンク","修道女","シスター","聖教者","クレリック","守門","オスティアリー","ドアキーパー","読師","レクター","侍祭","アコライト","祓魔師","悪魔祓い","エクソシスト","助祭","ディーコン","神父","司祭","プリースト","司教","ビショップ","大司教","アーチビショップ","高司祭","ハイプリースト","女教皇","ハイプリエステス","枢機卿","カーディナル","教皇","法王","ポープ","教皇","法王","ハイエロファント","殉教者","マーター","医者","ドクター","魔法医師","ウィッチドクター","治療師","ヒーラー","看護兵","メディック","薬師","薬草師","ハーバリスト","薬剤師","ファーマシスト","僧侶","職人","クラフトマン","鍛冶師","ブラックスミス","支配者","クエスター","創造主","クリエイター","料理人","シェフ","調香師","パフューマー","調教師","テイマー","あやつり師","パペットマスター","人形使い","ゴーレムマスター","野獣使い","ビーストマスター","ビーストテイマー","鷹匠","ファルコナー","ホーカー","道化師","クラウン","野伏","レンジャー","森林保護者","フォレスト","レンジャー","暗器使い","トリックスター","技師","メカニック","整備兵","リペアラー","機工士","マシーナリー","軽業師","アクロバット","案内人","ガイド","荷物持ち","ポーター","地図職人","マッパー","商人","マーチャント","行商人","ペドラー","従者","メイド","バトラー","探偵","ディテクティブ","交渉人","ネゴシエーター","学者","スカラー","教授","プロフェサー","科学者","サイエンティスト","踊り子","ダンサー","転送士","テレポーター","輸送兵","トランスポーター","吟遊詩人","バード","ミンストレル","強化者","ブースター","無法者","アウトロー","賭博師","ギャンブラー","詐欺師","スウィンドラー","犯罪者","カルプリット","不良","ヤンキー","ならず者","ごろつき","ローグ","フーリガン","悪徳商人","マーチャント","奴隷商人","人買い","闇商人","密偵","スパイ","協力者","ケースオフィサー","諜報員","エージェント","二重密偵","ダブルスパイ","密告者","インファーマー","裏切り者","ビトレイヤー","誘惑者","シデューサー","偽善者","ヒポクリット","傍観者","オンルッカー","暗殺者","アサシン","死刑執行人","異端児","異端審問官","インクゼーション","虐殺者","ジェノサイダー","盗賊","シーフ","夜盗","バークラー","怪盗","ファントム","追跡者","チェイサー","幻影","幽霊","ファントム","死霊","リッチ","暗黒","闇僧侶","ダークプリースト","暗黒","闇神官","暗黒騎士","ダークナイト","ブラックナイト","賞金稼ぎ","魔王","デーモンキング","スパイ","侍","サムライ","忍者","ニンジャ","くの一","クノイチ","山伏","ヤマブシ","巫女","ミコ","陰陽師","オンミョウジ","呪術師","イタコ","法師","ホウシ","傾奇者","バサラ","浪人","ロウニン","草履取り","ゾウリトリ","足軽","アシガル","足軽頭","足軽大将","アシガルガシラ","侍大将","武将","ブショウ","家老","カロウ","奉行","ブギョウ","大老","タイロウ","小名","ショウミョウ","大名","ダイミョー","関白","カンパク","摂政","セッショウ","太閤","タイカク","殿","トノ","帝","ミカド","悪代官","アクダイカン","町人","マチビト","チョウニン","商人","アキンド","丁稚","デッチ","番頭","バントウ","呉服屋","ゴフクヤ","八百屋","ヤオヤ","魚屋","ウオヤ","水茶屋","ミズチャヤ","町医者","マチイシャ","ヤブ医者","ヤブイシャ","薬売り","クスリウリ","絵師","エシ","彫金師","ホリモノシ","花火師","ハナビシ","力士","相撲取り","リキシ","スモウトリ","関取","セキトリ","飛脚","ヒキャク","岡っ引き","オカッピキ","舞妓","舞子","マイコ","芸者","芸妓","芸子","ゲイシャ","ゲイギ","ゲイコ","看板娘","カンバンムスメ","遊女","ユウジョ","花魁","オイラン","赤ちゃん","ベイビー","子供","チルドレン","若い","ヤング","年老いた","オールド","少年","少女","乙女","ギャル","メイデン","ギャル","紳士","ジェントルメン","淑女","レディー","小さな","リトル","見習い","アプレンティス","スペシャル","魔法","マジカル","マジック","聖人","セイント","聖","ホーリー","ホワイト","シャドウ","ダーク","ブラック","レッド","ブルー","悪魔","デビル","エビル","イビル","竜","ドラゴン","フォレスト","ルーキー","熟練","ベテラン","マスター","専門","エキスパート","指揮官","リーダー","師範","メンター","支配者","ルーラー","荘厳な","グランド",
    "アイドル","アーキビスト","アクチュアリー","アーティスト","アナウンサー","アニメーター","海人","アレンジャー","医師","石工","イタコ","板前","鋳物工","イラストレーター","医療監視員","医療事務員","医療従事者","医療保険事務","刺青師","インストラクター","ウェブデザイナー","鵜飼い","浮世絵師","宇宙飛行士","占い師","運転士","運転手","運転代行","映画監督","映画スタッフ","映画俳優","営業員","衛視","衛生検査技師","映像作家","栄養教諭","栄養士","駅員","駅長","絵師","エステティシャン","絵本作家","演歌歌手","園芸家","エンジニア","演出家","演奏家","オートレース選手","オプトメトリスト","お笑い芸人","お笑いタレント","音楽家","音楽評論家","音楽療法士","音響監督","音響技術者","海技従事者","会計士","外交官","外航客船パーサー","介護ヘルパー","海事代理士","会社員","海上自衛官","海上保安官","会長","介助犬訓練士","カイロプラクター","カウンセラー","画家","学芸員","科学者","学者","学生","学長","格闘家","菓子製造技能士","歌手","歌人","楽器製作者","学校事務職員","学校職員","学校用務員","活動弁士","家庭教師","カーデザイナー","歌舞伎役者","カメラマン","カラーセラピスト","為替ディーラー","環境デザイナー","環境計量士","看護師","看護助手","鑑定人","監督","官房長官","管理栄養士","官僚","議員","機関士","戯曲家","起業家","樵","棋士 ","棋士 ","記者","騎手","技術者","気象予報士","機長","キックボクサー","着付師","客室乗務員","脚本家","キャリア ","国家公務員","救急救命士","救急隊員","きゅう師","給仕人","厩務員","キュレーター","教育関係職員","教員","行政官","行政書士","競艇選手","教頭","教諭","銀行員","空間デザイナー","グランドスタッフ","グランドホステス","クリーニング師","クレーン運転士","軍事評論家","軍人","ケアワーカー","介護士","経営者","芸妓","経済評論家","警察官","芸術家","芸人","芸能人","芸能リポーター","警備員","刑務官","警務官","計量士","競輪選手","劇作家","ケースワーカー","ゲームデザイナー","ゲームライター","検疫官","研究員","言語聴覚士","検察官","検察事務官","現像技師","建築家","建築士","校閲者 -航海士","工業デザイナー","航空管制官","航空機関士","皇宮護衛官","航空自衛官","航空従事者","航空整備士","工芸家","講師 ","工場長","交渉人","講談師","校長","交通指導員","高等学校教員","公認会計士","公務員","校務員","港湾荷役作業員","国際公務員","国連職員","国税専門官","国務大臣","ゴーストライター","国会議員","国会職員","国家公務員","コピーライター","コミッショナー","コメディアン","コ・メディカル","コラムニスト","顧問","コンサルタント","コンシェルジュ","コンセプター","再開発プランナー","裁判官","裁判所職員","裁判所調査官","左官","作業療法士","作詞家","撮影監督","撮影技師","作家","サッカー選手","作曲家","茶道家","サラリーマン","参議院議員","指圧師","自衛官","シェフ","歯科医師","司会者","歯科衛生士","歯科技工士","歯科助手","士官","指揮者","司書","司書教諭","詩人","自然保護官","質屋","市町村長","実業家","自動車整備士","児童文学作家","シナリオライター","視能訓練士","司法書士","事務員","社会福祉士","社会保険労務士","車掌","写真家","写真ディレクター","社長","ジャーナリスト","写譜屋","獣医師","衆議院議員","臭気判定士","柔道整復師","守衛","塾講師","手話通訳士","准看護師","准教授","小学校教員","証券アナリスト","将校","小説家","消防官","照明技師","照明技術者","照明士","照明デザイナー","書家","助教","助教授","職人","ショコラティエ","助手 ","初生雛鑑別師","書道家","助産師","神職","審判員","新聞記者","新聞配達員","心理カウンセラー","診療放射線技師","心理療法士","樹医","随筆家","推理作家","スカウト ","寿司職人","スタイリスト","スタントマン","スチュワーデス","スチュワード","スパイ","スーパーバイザー","スポーツ選手","スポーツドクター","摺師","製菓衛生師","声楽家","税関職員","政治家","聖職者","整体師","青年海外協力隊員","整備士","声優","税理士","セックスワーカー","セラピスト","船員","選挙屋","船長","戦場カメラマン","染織家","潜水士","造園家","葬儀屋","造形作家","相場師","操縦士","装丁家","僧侶","測量士・測量技師","速記士","ソムリエ","ソムリエール","村議会議員","大学教員","大学教授","大学職員","大工","大臣","大道芸人","大統領","ダイバー","殺陣師","旅芸人","タレント","ダンサー","探偵","チェリスト","知事","地方議会議員","地方公務員","中学校教員","中小企業診断士","調教師","調香師","彫刻家","聴導犬訓練士","著作家","通関士","通信士","通訳","通訳案内士","ディスパッチャー","ディーラー","ディレクター","テクノクラート","デザイナー","テニス選手","電気工事士","電車運転士","添乗員","電話交換手","陶芸家","投資家","杜氏","動物看護師","動物管理官","時計師","登山家","図書館司書","鳶職","トラックメイカー","トリマー","ドリラー","トレーナー","内閣官房長官","内閣総理大臣","仲居","ナニー","ナレーター","入国警備官","入国審査官","庭師","塗師","ネイリスト","農家","能楽師","納棺師","配管工","俳人","バイヤー","俳優","パイロット","バスガイド","パタンナー","発明家","パティシエ","バーテンダー","噺家","花火師","花屋","はり師","バリスタ ","パン屋","ピアノ調律師","美術 ","美術家","美術商","秘書","筆跡鑑定人","ビデオジョッキー","ビューロクラート","美容師","評論家","ビル管理技術者","ファシリテーター","ファンタジー作家","ファンドレイザー","風俗嬢","フェロー","副校長","服飾デザイナー","副操縦士","腹話術師","舞台演出家","舞台監督","舞台俳優","舞台美術家","舞踏家","武道家","不動産鑑定士","不動産屋","舞踊家","プラントハンター","ブリーダー","振付師","フリーライター","プログラマ","プロゴルファー","プロデューサー","プロブロガー","プロボウラー","プロボクサー","プロ野球選手","プロレスラー","文芸評論家","文筆家","フライス盤工","ベビーシッター","編曲家","弁護士","編集者","弁理士","保安官","保育士","冒険家","放射線技師","宝飾デザイナー","放送作家","法務教官","訪問介護員","牧師","保険計理人","保健師","保護観察官","ホステス","ホスト","ボディーガード","ホームヘルパー","ホラー作家","彫師","翻訳家","舞妓","マジシャン ","マーシャラー","マタギ","マッサージ師","マニピュレーター","マルチタレント","漫画家","漫画原作者","漫才師","漫談家","ミキサー","巫女","水先案内人","水先人","宮大工","ミュージシャン","無線通信士","メイド","メジャーリーガー","盲導犬訓練士","モデラー ","モデル ","薬剤師","役者","野菜ソムリエ","郵便配達","YouTuber","洋菓子職人","養護教諭","洋裁師","養蚕家","幼稚園教員","養蜂家","ライトノベル作家","ライフセービング","落語家","酪農家","ラグビー選手","理学療法士","力士","陸上自衛官","リポーター","猟師","漁師","理容師","料理研究家","料理人","旅行作家","林業従事者","臨床検査技師","臨床工学技士","臨床心理士","ルポライター","レーサー","レスキュー隊員","レポーター","レンジャー","労働基準監督官","録音技師","和菓子職人","和裁士","和紙職人","A&R","CMディレクター","DJ","MR","PAエンジニア","SF作家","SP"
]

const NAMES:string[] = [
    "アーヴィング","アーソリン","アーク","アータム","アーサー","アーブリー","アーサック","アーライ","アーネスト","アーリア","アーノルド","アーリイ","アーバン","アイシャ","アーマド","アイビー","アーメッド","アイヤナ","アール","アイラ","アーロン","アイリーン","アイザアス","アイリーン","アイザック","アイリーン","アイザック","アイリーン","アイデン","アイリス","アイデン","アジア","アウグストゥス","アシャ","アグスティン","アシュティン","アクセル","アシュトン","アシュトン","アシュリー","アダマン","アシュリー","アダム","アシュリー","アッシャー","アシュリー","アディソン","アシュリン","アデン","アスペン","アドニス","アディソン","アドリアン","アテナ","アドルフォ","アデリン","アブドラ","アドリアーナ","アブラハム","アドリアンナ","アブラム","アドリアンヌ","アベル","アナ","アマリ","アナスタシア","アミール","アナナリー","アモス","アナヒ","アラン","アナベル","アラン","アナベル","アリ","アナヤ","アリ","アナリーズ","アリエル","アニー","アルヴァロ","アニカ","アルヴィン","アニカ","アルデン","アニサ","アルド","アニタ","アルトゥーロ","アニヤ","アルバート","アニワ","アルフォンソ","アネット","アルフレッド","アビー","アルフレド","アビー","アルベルト","アビー","アルマーニ","アビガイル","アルマンド","アビガレ","アレキサンダー","アビゲイル","アレクサンダー","アビゲイル","アレクサンドロ","アビゲル","アレクシス","アブロー","アレック","アマニ","アレック","アマヤ","アレックス","アマラ","アレッサンドロ","アマリ","アレハンドロ","アマンダ","アレン","アミーナ","アロン","アミーヤ","アロンソ","アミラ","アロンゾ","アメリア","アンジェロ","アメリカ","アンソニー","アヤ","アンダーソン","アヤ","アンディ","アヤナ","アントニー","アヤナ","アントニオ","アライシャ","アンドリュー","アライソン","アンドレ","アラエナ","アンドレアス","アラサリー","アンドレス","アラサリー","アントワーヌ","アラナ","アントン","アランナ","アントン","アリ","イアン","アリア","イアン","アリア","イーサン","アリアナ","イーストン","アリアンナ","イエス","アリアンナ","イグナシオ","アリー","イザイ","アリーア","イザヤ","アリーシャ","イザヤ","アリーナ","イザヤ","アリエル","イシドロ","アリエル","イスマエル","アリサ","イスラエル","アリサ","イブラヒム","アリザ","イワン","アリシア","インファント","アリシャ","ヴァーノン","アリス","ヴィセンテ","アリゼ","ウィリアム","アリソン","ウィリー","アリソン","ウィル","アリソン","ウィルソン","アリッサ","ウィンストン","アリッサ","ヴィンセント","アリッサ","ヴィンチェンツォ","アリナ","ウェイド","アリビア","ウェイン","アリヤ","ウェストン","アリヤー","ウェズリー","アルマ","ウォーカー","アルマーニ","ウォーレン","アルレーン","ヴォーン","アレクサ","ウォルター","アレクサス","ウリエル","アレクサンドラ","ウンベルト","アレクサンドリア","エイヴリー","アレクサンドリア","エイサ","アレクシア","エイダン","アレクシス","エイドリアン","アレクシス","エヴァン","アレジャンドラ","エヴェレット","アレックス","エズキエル","アレッサンドラ","エステバン","アレナ","エステバン","アロンドラ","エズラ","アン","エゼキエル","アン","エディ","アンジー","エドゥアルド","アンジェラ","エドウィン","アンジェリーク","エドガー","アンジェリーナ","エドワード","アンジェリカ","エフレイン","アンジャリ","エフレイン","アンズレー","エマーソン","アントニア","エマニュエル","アンドレア","エマニュエル","アントワネット","エミリアーノ","アンナ","エミリオ","アンバー","エメット","イヴェット","エリ","イヴォンヌ","エリアス","イェーナ","エリアン","イエセニア","エリオット","イエメニア","エリオット","イサベラ","エリシャ","イサベラ","エリス","イザベラ","エリゼオ","イザベル","エリック","イザベル","エリック","イシス","エリック","イッツェル","エリヤ","イブ","エルヴィス","イマニ","エルナン","イヤナ","エルネスト","イレーヌ","エルマー","イワナ","エレミヤ","イングリッド","エンジェル","インディア","エンリケ","ヴァネッサ","オーウェン","ヴァレリア","オーガスト","ヴァレリー","オースティン","ヴィヴィアナ","オースティン","ヴィヴィアン","オースティン","ウィロー","オーブリー","ヴェロニカ","オーランド","ウェンディ","オクタビオ","エイヴリー","オスカー","エイジャ","オズボルド","エイドリアン","オマール","エイプリル","オマリ","エイミー","オリオン","エイミー","オリバー","エヴァ","エヴリン","エーヴァ","エスター","エステファニア","エストレラ","エスメラルダ","エスメラルダ","エッセンス","エディス","エデン","エボニー","エマ","エマリー","エミリア","エミリー","エミリー","エミリー","エメラルド","エメリー","エラ","エリ","エリアナ","エリーゼ","エリカ","エリカ","エリザ","エリザ","エリザベス","エリザベス","エリゼ","エリッカ","エリッサ","エリッサ","エリナ","エリン","エリン","エルサ","エレナ","エレノア","エレン","エンジェル","オードリー","オーブリー","オーロラ","オダリス","オリビア","カーク","カーソン","カーソン","カーラ","カーソン","カーリー","カーター","カーリー","カーティス","カーリー","カーティス","カーリイ","カール","カール","カール","カイア","カールトン","カイトリン","カイ","カイトリン","ガイ","カイトリン","カイディン","カイラ","カイデン","カイラー","カイデン","カイラン","カイド","カイリ","カイラー","カイリー","カイル","カイリー","カディン","カイリー","カデン","カイリー","カデン","カイリン","ガブリエル","カイリン","カボン","カイリン","カムデン","カイリー","カムレン","カエラ","カムロン","カサンドラ","カムロン","カサンドラ","カメロン","カザンドラ","カリル","カシー","カルト","カタリナ","カルバン","カタリナ","カルロ","カチ","カルロス","カッサンドラ","ガレット","カッシディ","ガレット","カティア","カレブ","カテリーナ","カレブ","カテリン","カレン","カトリーナ","カレン","カトリン","ガンナー","カトリン","ガンナー","ガブリエラ","キアヌ","ガブリエラ","キアン","ガブリエル","キーオン","ガブリエル","キーガン","カミーユ","キース","カミーラ","キートン","カムリン","キーホーン","カムリン","キエン","カメロン","ギデオン","カヤ","キナン","カラ","ギャビン","カラ","ギャビン","カリ","キャメロン","カリ","キャメロン","カリ","ギャリソン","カリ","ギャレット","カリ","キュルス","カリ","キラン","カリ","キリアン","カリ","ギルバート","カリー","ギレルモ","カリー","グアダルーペ","カリーナ","クイン","カリサ","クインシー","カリスタ","クインチン","カリッサ","クインティン","カリナ","クイントン","カリン","クーパー","カリン","クエンティン","カルメン","グスタボ","カルラ","クラーク","カレン","グラハム","キアナ","クラレンス","キアナ","グラント","キアラ","クリス","キアラ","クリスチャン","キーラ","クリスチャン","キーリー","クリスチャン","キエステン","クリストバル","キエラ","クリストファー","キエラ","クリストファー","ギッセル","クリストファー","キャサリン","クリストファー","キャサリン","グリフィン","キャサリン","クリフォード","キャサリン","クリフトン","キャシー","クリント","キャシー","クリントン","キャシディ","クルーズ","キャスリーン","クレイ","キャメロン","クレイグ","キャリー","グレイソン","キャリー","グレイソン","キャリー","グレイディ","キャロライナ","クレイトン","キャロライン","グレゴリー","キャロリン","グレン","キャロル","グレン","キャンディス","ゲイリー","キャンディス","ケイン","キラ","ケヴェン","キラ","ゲージ","キラ","ゲージ","キリー","ケーシー","キルステン","ケーシー","キルステン","ケード","キンシー","ケール","キンバリー","ケール","グアダルーペ","ケガガン","クイン","ケシャン","グウェンドリン","ケシャン","クラウディア","ケニー","グラシエラ","ケニヨン","クララ","ケネス","クラリッサ","ケネディ","クリスタ","ケビン","クリスタ","ケボン","クリスタル","ケリー","クリスタル","ケルトン","クリスタル","ケルビン","クリスチャン","ケレン","クリスティ","ケント","クリスティ","ケンドール","クリスティアーナ","ケンドリック","クリスティーナ","コーディ","クリスティーナ","コーデル","クリスティーナ","コートニー","クリスティーヌ","ゴードン","クリスティン","コーナー","クリスティン","コーナー","クリステン","コーネリアス","クレア","コービン","クレア","コーリー","グレイシー","コール","グレース","コール","グレタ","コールマン","グレッチェン","コディー","クロエ","コナー","グロリア","コビー","ケアア","コビー","ケイシー","コビー","ケイティ","コリー","ケイティ","コリー","ケイト","コリー","ケイトリン","コリン","ケイトリン","コリン","ケイトリン","コルテス","ケイトリン","コルテン","ケイトリン","コルト","ケイラ","コルトン","ケイリー","コルトン","ケイリー","コルビー","ケイリー","コルビー","ケイリー","コルビン","ケイリー","ゴンザロ","ケイリン","コンラッド","ケーシー","ケーシー","ケッリ","ケナ","ケニア","ケニア","ケネディ","ケネディ","ケリー","ケリー","ケリー","ケリー","ケルシ","ケルシ","ケルシー","ケンジ","ケンドール","ケンドラ","コートニー","コートニー","コーラ","コリーン","コリーン","コルニー","コロ","ザイール","サイゲ","ザイオン","サシャ","サイモン","サディー","サヴィオン","サバナ","サウル","サバナ","ザカリー","サバンナ","ザカリー","サバンナ","ザカリヤ","サブリナ","ザッカーリー","サマー","ザッケリー","サマラ","サニー","サマンサ","サバスティアン","サラ","ザビエル","サラ","ザビエル","サライ","ザヘリー","ザリア","サミー","サリー","サミール","サリーナ","サミュエル","サルマ","サム","サンディ","サムソン","サンドラ","サヤド","ジア","サルヴァトーレ","シアラ","サルバドール","ジアンナ","ザンダー","シイラ","ザンダー","シェア","サンティアゴ","ジェイダ","サントス","ジェイデン","シーザー","ジェイド","シーマス","ジェイド","ジーン","ジェイミー","シェア","シェイラ","ジェイ","ジェイラ","ジェイク","シェイリー","ジェイコブ","ジェイリン","ジェイシー","ジェーン","ジェイス","ジェシー","ジェイソン","ジェシー","ジェイソン","ジェシー","ジェイデン","ジェシカ","ジェイドン","シエナ","シェイネ","ジェナ","ジェイミー","ジェニー","ジェイムソン","ジェニファー","ジェイラン","ジェニファー","ジェイリン","ジェネシス","ジェイリン","シエラ","ジェイロン","シエラ","ジェヴァン","シエラ","ジェームス","シェリダン","シェーン","シェルビー","ジェシー","シエロ","ジェシー","ジゼル","ジェット","シディニ","ジェナロ","シドニー","ジェフ","シドニー","ジェファーソン","シドニー","ジェフリー","シドニー","ジェフリー","シドニー","ジェフリー","シトラリイ","シェマール","ジナ","ジェラルド","ジネット","ジェラルド","シモーヌ","ジェラルド","ジャーデン","ジェリー","シャーナ","シェルドン","シャーリー","ジェレミー","シャーロット","ジェローム","シャイアン","シドニー","シャイアン","ジノ","シャイアン","ジミー","ジャイセ","ジミー","ジャイダ","シメオン","シャイナ","ジャーデン","ジャイリン","ジャーデン","ジャカイラ","ジャービス","シャキーラ","シャーマール","ジャクリーン","ジャーマン","ジャクリーン","ジャーメイン","ジャクリン","ジャイロ","ジャクリン","ジャクァン","ジャケリン","ジャクソン","ジャケリン","ジャクソン","ジャスティス","ジャクソン","ジャスティン","ジャスタス","ジャスミン","ジャスティス","ジャスミン","ジャスティン","ジャスミン","ジャスティン","ジャズミン","ジャスパー","ジャズミン","ジャッキー","ジャズミン","ジャック","ジャズミン","ジャデン","ジャズリン","ジャドン","ジャダ","シャノン","ジャックリン","ジャバリ","ジャディン","ジャボン","ジャデン","ジャボンテ","シャナイア","ジャマー","ジャニス","ジャマールス","シャニヤ","ジャマリ","ジャネッサ","ジャマル","ジャネット","ジャミール","ジャネッレ","ジャミソン","シャネル","ジャミル","シャノン","ジャメル","シャヤン","ジャレット","ジャリン","ジャレド","ジャレイン","ジャレド","シャロン","ジャレン","シャンテル","ジャレン","ジュエル","ジャロッド","ジュディス","ジャロド","ジュヌビエーブ","ジャロド","ジュリア","ジャロン","ジュリアナ","ジャン","ジュリアンナ","ジャンカルロ","ジュリアンヌ","ジャンニ","ジュリー","ジュード","ジュリエット","ジュニア","ジュリエット","ジュリアス","ジュリッサ","ジュリアン","ジョアナ","ジュリアン","ジョアン","ジョヴァニー","ジョアンナ","ジョエル","ジョイ","ジョー","ジョイス","ジョーイ","ジョヴァンナ","ジョージ","ショウナ","ジョーダン","ジョエル","ジョーダン","ジョーイ","ジョーディ","ジョージア","ショーン","ジョーダン","ショーン","ジョシー","ショーン","ジョセフィン","ジョーン","ジョセリン","ジョシュ","ジョセリン","ジョシュア","ジリアン","ジョスエ","ジリアン","ジョセフ","ジル","ジョセフ","シルビア","ジョナ","シルビア","ジョナサン","シンシア","ジョナサン","シンディ","ジョナス","スーザン","ジョナトン","スカーレット","ジョニー","スカイ","ジョニー","スカイ","ジョバニ","スカイラ","ジョバン","スカイラ","ジョバンニ","スカイラー","ジョバンニ","スサナ","ジョン","ステイシー","ジョン","ステイシー","ジョンナトン","ステファニー","シラス","ステファニー","ジルベルト","ステファニー","シンシア","ステラ","スカイラ","セージ","スカイラー","セクリア","スコット","セシリア","スターリング","セリーナ","スタンリー","セリーナ","スチュワート","セリーヌ","スティーヴン","セレステ","スティーブ","セレナ","スティーブン","セレナ","ステファン","セレニティ","ステファン","セレリア","ステホン","ゾーイ","ストーン","ゾーイ","スペンサー","ゾーイ","セージ","ソニア","ゼーン","ソニア","セオドア","ソフィア","ゼカリヤ","ソフィア","セス","ソフィー","セドリック","セバスチャン","セルジオ","ソーヤー","ソロモン","ターナー","ダービー","ダーリン","ダイアナ","ダーリン","ダイアン","タイ","ダイシャ","タイソン","ダイジャ","タイタス","ダイヤモンド","タイラー","タイラ","タイリク","タイラー","タイリク","ダコタ","タイレク","ダシア","タイレル","ダズリー","タイロル","ダズリー","タイロン","タタム","ダイロン","タチアナ","ダヴォン","タチアナ","ダヴォンテ","タチアナ","ダクアン","ダナ","ダグラス","タニア","ダコタ","ダニエッラ","ダショーン","ダニエラ","ダスティン","ダニエル","タッカー","タニヤ","タデウス","タビスタ","タナー","ダフネ","ダニー","タマラ","ダニエル","ダマリス","ダネル","タミア","ダビオン","タヤ","ダミアン","タラ","ダミアン","ダラス","ダメージ","タリア","ダライアス","タリア","ダラス","ダリアナ","ダリアス","ダリアン","ダリアン","タリーヌ","タリース","タリサ","ダリエン","ダルセ","ダリオ","ダレーネ","ダリオン","ダン","ダリオン","チェルシー","タリク","チェルシー","ダリル","チャイナ","ダリル","チャヤ","ダリン","チャリティー","ダルトン","チャリティー","ダレル","チャンドラー","ダレン","デアシア","タロン","ティア","ダン","ティアナ","ダンカン","ティアナ","ダンゲロ","ディアナ","ダンテ","ディアナ","ダンデル","ティアラ","チェイス","ティーガン","チャーリー","ティエラ","チャールズ","デイジー","チャイム","ティナ","チャズ","デイナ","チャド","デイナナ","チャンス","ティファニー","チャンドラー","テイラー","ディアンジェロ","テイラー","ディアンドレ","デヴィン","ディーン","デヴィン","ディエゴ","デジャ","ディオン","デジャ","ティソーン","テス","テイト","デスタニー","デイトン","デスティニ","デイビス","デスティニー","ディマルカス","デスティニー","ディマルコ","デスティニー","ディミトリ","テッサ","ティモシー","デニス","デイモン","デボラ","テイラー","デボン","ディラン","デラニー","ディラン","デリア","ティリー","デリラ","ディリオン","テレサ","ディロン","トーリー","デヴァン","ドナ","デヴァンテ","トニー","テヴィン","ドミニク","デヴィン","トリシャ","デヴィン","トリニティ","デーネ","ドリュー","デービン","トレーシー","デール","ドロシー","デーン","デオン","デオン","デオンテ","デオンテエ","デクスター","デクラン","デスティン","デスハウン","デスモンド","デニス","デハーン","デビッド","デブン","デボン","デボンテ","デメトリアス","デュアン","テランス","テリー","デリック","デリック","デレク","テレル","テレンス","テレンス","デワイン","デンゼル","デンドレ","デンバー","ドウェイン","ドーソン","トーマス","トッド","ドナバン","ドナボン","ドナルド","トニー","ドニー","ドノバン","トバイアス","トビー","トマス","トミー","ドミニク","ドミニク","ドミニク","ドミニク","ドミニク","トラビス","トラビス","ドリアン","トリスタン","トリスタン","トリスタン","トリスチン","トリステン","トリストン","ドリュー","トレ","トレイ","トレイヴォン","ドレイヴン","ドレイク","トレース","トレバー","トレバー","トレビオン","トレビオン","トレント","トレントン","トロイ","ドワイト","ドン","ドンタエ","ドンテ","ドンネル","ナイジェル","ナエリ","ナサニアル","ナオミ","ナサニエル","ナターシャ","ナシール","ナタリア","ナタナエル","ナタリー","ナチェン","ナタリー","ナッシュ","ナタリー","ニール","ナディア","ニール","ナディーン","ニクラウス","ナンシー","ニコ","ニア","ニコラス","ニーナ","ニコラス","ニキータ","ニコラス","ニコール","ニコラス","ニコル","ニック","ニコレット","ニヒル","ニッキー","ネイサン","ニャ","ネストール","ニャ","ネヘミヤ","ニャシア","ネルソン","ネハ","ノア","ノエミ","ノエ","ノエリア","ノエル","ノエル","ノーマン","ノラ","ノーラン","ノルマ","パーカー","パーカー","バーナード","バージニア","ハーバート","バーバラ","ハーベイ","ハーモニー","ハーレー","ハーレー","ハイメ","バイオレット","バイロン","ハイジ","パクストン","パイパー","ハッサン","ハイメ","ハドソン","パオラ","パトリック","ハドリー","ハビエル","パトリシア","ハビオン","ハナ","パブロ","パメラ","ハムザ","パリ","パリ","ハリー","ハリー","ハリー","バリー","ハレ","ハリソン","ハレイ","ハリド","バレンティーナ","バレット","パロマ","バレンティン","ハンター","ハロルド","ハンナ","ハワード","ハンナ","ハンター","ビアトリス","ピアース","ビアンカ","ヒース","ビクトリア","ピーター","ヒラリー","ピエール","ファチマ","ビクター","フアニータ","ヒュー","ファビオラ","ヒューゴ","フィービー","ヒューストン","フィオナ","ビリー","フェイス","ファビアン","フェリシア","ファン","フェリシティ","フィデル","フェルナンダ","フィリップ","ブライアーナ","フィリップ","ブライアン","フィン","ブライアン","フェニックス","ブライアン","フェリックス","ブランカ","フェリペ","フランシス","フェルナンド","フランチェスカ","フォーレスト","ブランディ","ブライアン","ブランデー","ブライアン","ブリア","ブライアン","ブリアナ","ブライアント","ブリアン","ブライス","ブリアンナ","ブライス","ブリーレ","ブライセン","ブリオンナ","ブライソン","ブリサ","ブライドン","ブリジット","ブラクストン","ブリジット","ブラッド","プリシラ","ブラッドリー","ブリトニー","ブラッドリー","ブリトニー","フランキー","ブリン","フランク","プリンセス","フランクリン","ブルターニュ","フランシス","ブルック","フランシスコ","ブルック","ブランソン","ブルックリン","ブランダン","ブルックリン","フランチェスコ","ブレア","ブランディン","ブレアナ","ブランデン","プレシャス","ブラント","プレスリー","ブランドン","ブレナ","フリオ","ブレナ","プリンス","ブレンダ","ブルース","ベアトリス","ブルーノ","ペイシャンス","ブルックス","ヘイデン","ブレイク","ペイトン","ブレイズ","ペイトン","ブレイズ","ベイビー","ブレイディ","ヘイリー","ブレイデン","ヘイリー","ブレイトン","ヘイリー","ブレイン","ヘイリー","ブレーデン","ヘイリー","ブレーデン","ヘイリー","ブレーデン","ヘイリー","プレストン","ベイリー","フレッド","ベイリー","ブレット","ベイリー","ブレット","ページ","フレッドリック","ヘーゼル","フレディ","ヘザー","フレディ","ベサニー","フレデリック","ヘブン","ブレナン","ヘブン","ブレネン","ベラ","ブレノン","ペルラ","ブレンダン","ヘレナ","ブレンデン","ベレニス","ブレント","ヘレン","ブレントン","ベレン","ブレンドン","ホイットニー","ブロック","ホープ","ブロディ","ポーラ","ブロディ","ポーリーナ","ブロンソン","ボニー","ヘイデン","ホリー","ペイトン","ペイトン","ベイビー","ベイリー","ヘクター","ペドロ","ベニー","ベニート","ベネット","ペリー","ヘリバート","ベルナルド","ベン","ベンジャミン","ヘンリー","ホアキン","ボー","ボー","ポーター","ポール","ホールデン","ホセ","ボビー","ホルヘ","マーヴィン","マーガレット","マーカス","マーサ","マーキス","マーリー","マーキス","マーリー","マーク","マイア","マーク","マイヤ","マーシャル","マイラ","マーティン","マウラ","マーベリック","マガン","マーロン","マギー","マイク","マグダレーナ","マイケル","マケイラ","マイケル","マケナ","マイル","マセイ","マイルズ","マダリン","マイロン","マチ","マウリシオ","マッカイラ","マキシミリアーノ","マッケンジー","マキシム","マッケンジー","マキシムス","マッティ","マクシミリアン","マディシン","マクシミリアン","マディセン","マクスウェル","マディソン","マシュー","マディソン","マシュー","マディソン","マックス","マディリン","マッケンジー","マデリーン","マッテオ","マデリン","マテオ","マデリン","マヌエル","マドレーヌ","マラキ","マヤ","マリアーノ","マヤ","マリオ","マラ","マリク","マライア","マルクス","マランダ","マルケス","マリア","マルケス","マリア","マルコ","マリアナ","マルコス","マリアム","マルコム","マリアム","マルセル","マリアン","マルセロ","マリアンナ","ミカ","マリー","ミカイ","マリーナ","ミケル","マリエラ","ミゲル","マリサ","ミサエル","マリソル","ミッチェル","マリツァ","ミッチェル","マリッサ","ミルトン","マリベル","ムハンマド","マリリン","メイソン","マルガリータ","メルビン","マルティナ","モイセス","マルレン","モーガン","マレーネ","モーセ","マロリー","モーリス","マンディー","モシェ","ミア","モハマド","ミア","モハメッド","ミア","モハメド","ミカ","ミカイラ","ミカエラ","ミカエラ","ミカエルダ","ミカンジー","ミシェル","ミシェル","ミスティ","ミラクル","ミランダ","ミリアム","ミレヤ","メアリー","メイガン","メイガン","メイシー","メイシー","メイブ","メーガン","メガン","メッケンナ","メラニー","メリッサ","メリッサ","メリナ","メリンダ","メルセデス","メレディス","メロディー","モーガン","モニーク","モニカ","モリアー","モリー","モリー","モンセラット","モンタナ","ヤコビ","ヤスミーン","ヤコブ","ヤスミン","ユージーン","ヤスミン","ユダ","ヤスミン","ユリシーズ","ヤディラ","ユリシーズ","ヤナ","ヨシア","ヤナエ","ヨハン","ヤニヤ","ヨルダン","ヤミレ","ユニーク","ヨハンナ","ヨランダ","ヨルダン","ライアン","ライアン","ライダー","ライアン","ライリー","ライシャ","ライリー","ライナ","ラウル","ライラ","ラシード","ライリー","ラシャド","ライリー","ラショーン","ライリー","ラッセル","ララ","ラディリアス","ラリー","ラトレル","ラリッサ","ラファエル","ランディ","ラファエル","リア","ラフィム","リアーナ","ラフル","リアーナ","ラマー","リアーナ","ラミロ","リアーナ","ラモン","リー","ラモント","リーア","ラリー","リース","ラリー","リーン","ラルフ","リヴェン","ランス","リケル","ランダル","リサ","ランダン","リジー","ランダン","リズベス","ランディ","リゼス","リアム","リゼット","リー","リゼット","リース","リタ","リード","リディア","リード","リナ","リード","リベカ","リーン","リャノン","リーン","リラ","リカルド","リリアーナ","リゴベルト","リリアーナ","リチャード","リリアン","リッキー","リリアン","リッキー","リリー","リック","リリー","リバー","リリー","リラン","リリー","リロイ","リリック","リンカーン","リンジー","ルイ","リンゼイ","ルイス","リンゼー","ルイス","リンダ","ルーカス","ルイサ","ルーカス","ルーシー","ルーク","ルース","ルーベン","ルース","ルーベン","ルチア","ルカ","ルネー","ルチアーノ","ルビー","ルディ","ルルド","ルネ","レイガン","レイ","レイチェル","レイ","レイチェル","レイヴァン","レイチェル","レイナルド","レイナ","レイモンド","レイナ","レイモンド","レイナ","レヴィ","レイラ","レーガン","レイラ","レオ","レイラニ","レオナルド","レーガン","レオナルド","レーシー","レオネル","レーン","レオン","レガン","レジナルド","レクサス","レット","レクシ","レミントン","レクシー","ロイ","レジーナ","ロイス","レスリー","ロイド","レスリー","ローガン","レスリー","ローソン","レティシア","ローハン","レナ","ローマ","レベッカ","ローランド","レベッカ","ローランド","ローガン","ローレンス","ローザ","ローワン","ローズ","ロジェリオ","ローズマリー","ロジャー","ローラ","ロス","ローラル","ロッキー","ローリン","ロデリック","ローレン","ロドニー","ローレン","ロドリゴ","ロクサーヌ","ロドルフォ","ロザリンダ","ロナルド","ロシオ","ロナルド","ロビン","ロニー","ロビン","ロニー","ロリ","ロバート","ロレナ","ロベルト","ロンドン","ロミオ","ロリー","ロレンツォ","ロンドン","ワイアット"
]

