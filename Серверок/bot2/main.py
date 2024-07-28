import sqlite3
from aiogram import Bot, Dispatcher, executor, types
from aiogram.types import InlineKeyboardMarkup, InlineKeyboardButton, ReplyKeyboardMarkup, KeyboardButton, WebAppInfo
from aiogram.dispatcher.filters.state import State, StatesGroup
from aiogram.contrib.middlewares.logging import LoggingMiddleware
from aiogram.contrib.fsm_storage.memory import MemoryStorage
from aiogram.dispatcher import FSMContext
from keyboards import kb
import time
from apscheduler.schedulers.asyncio import AsyncIOScheduler

ADMINS = [1328149214, 5769375343, 835870766, 6967998352]
# API_TOKEN = '6941795038:AAHaZQdt-OCEzpjyS02PA-koq4ozlCEJRF4'
API_TOKEN = '7361307312:AAF8UeLRXZ_JGOfT6SR6OZDK2bArwYYKclM'

CHANNEL_ID = -1002034954233  # Укажите ID вашего приватного канала
# CHANNEL_ID = -1002085987521  # Укажите ID вашего приватного канала
INVITE_LINK = 'https://t.me/southpackcoin'  # Ссылка-приглашение в приватный канал
# INVITE_LINK = 'https://t.me/+_KMLTUSiGrs2ZDFi'  # Ссылка-приглашение в приватный канал
NAME_BOT = 'SouthPack_robot'  # Имя бота
# NAME_BOT = 'LogPulse_bot'  # Имя бота
bot = Bot(token=API_TOKEN, parse_mode='HTML')
storage = MemoryStorage()
dp = Dispatcher(bot, storage=storage)
dp.middleware.setup(LoggingMiddleware())
scheduler = AsyncIOScheduler()


# Создаем базу данных и таблицу пользователей
conn = sqlite3.connect('users.db')
cursor = conn.cursor()
cursor.execute('''CREATE TABLE IF NOT EXISTS users (
                    user_id INTEGER PRIMARY KEY,
                    language TEXT,
                    balance REAL DEFAULT 0,
                    referrals INTEGER DEFAULT 0,
                    referral_link TEXT,
                    date TEXT,
                    referrer_id INTEGER,
                    last_mine_time INTEGER,
                    name TEXT,
                    wallet TEXT DEFAULT 0,
                    per_day INTEGER DEFAULT 0,
                    per_month INTEGER DEFAULT 0,
                    notification INTEGER DEFAULT 0
               )''')
conn.commit()

# Получение информации о структуре таблицы
cursor.execute("PRAGMA table_info(users)")
columns = cursor.fetchall()

# Преобразование списка колонок в список имен колонок
column_names = [column[1] for column in columns]

# Проверка наличия колонок и добавление их при необходимости
if 'weapon' not in column_names:
    cursor.execute('ALTER TABLE users ADD COLUMN weapon FLOAT DEFAULT 0')

if 'soft_coins' not in column_names:
    cursor.execute('ALTER TABLE users ADD COLUMN soft_coins FLOAT DEFAULT 0')

conn.commit()



def can_collect2(user_id):
    cursor.execute('SELECT last_mine_time, balance, referrals FROM users WHERE user_id = ?', (user_id,))
    row = cursor.fetchone()

    if row is None:
        print("User not found")
        return False

    last_mine_time, balance, referrals = row
    current_time = int(time.time())  # Используем текущее время
    elapsed_time, new_coins, h, m = calculate_mine_pool(last_mine_time, current_time, referrals)
    max_coins = 50 + int(referrals)*5  # Максимальное количество монет для сбора
    language = cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id, )).fetchone()[0]
    if language == 'en':
        text2 = f"Current Mine Pool ($PACK): {new_coins:.2f} / {max_coins}"  # Показываем текущее количество монет с точностью до двух знаков
    else:
        text2 = f"Текущий Майнинг Пул ($PACK): {new_coins:.2f} / {max_coins}"  # Показываем текущее количество монет с точностью до двух знаков
    
    if elapsed_time >= 3 * 3600:  # Если прошло 4 часа или более
        # Разрешаем сбор монет и обновляем баланс
        updated_balance = balance + new_coins
        cursor.execute('UPDATE users SET notification = 0 WHERE user_id = ?', (user_id, ))
        conn.commit()
        cursor.execute('UPDATE users SET balance = ?, last_mine_time = ? WHERE user_id = ?',
                       (updated_balance, current_time, user_id))
        conn.commit()
        cursor.execute("UPDATE users SET per_day = per_day + ? WHERE user_id=?",  (new_coins, user_id,))
        conn.commit()
        cursor.execute("UPDATE users SET per_month = per_month + ? WHERE user_id=?",  (new_coins, user_id,))
        conn.commit()
        refferr = cursor.execute("SELECT referrer_id FROM users WHERE user_id=?", (user_id,)).fetchone()
        if refferr[0] != None:
            
            cursor.execute("UPDATE users SET balance = balance + ? WHERE user_id=?",
                        ((float(new_coins)*0.1), refferr[0],))
            conn.commit()
            cursor.execute("UPDATE users SET per_day = per_day + ? WHERE user_id=?",  ((float(new_coins)*0.1), refferr[0],))
            conn.commit()
            cursor.execute("UPDATE users SET per_month = per_month + ? WHERE user_id=?",  ((float(new_coins)*0.1), refferr[0],))
            conn.commit()
            
        if language == 'en':
            text = '$PACK collected, balance updated'
        else:
            text = '$PACK получены, баланс обновлен'
        return True, text, text2
    else:
        # Недостаточно времени прошло для сбора монет
        if language == 'en':
            text = '⚠️Not enough time has passed to collect coins(3 hours)'
        else:
            text = '⚠️Недостаточно времени прошло для сбора монет (3 часа )'
        return False, text, text2

def can_collect(user_id):  
    cursor.execute('SELECT last_mine_time, balance, referrals FROM users WHERE user_id = ?', (user_id,)) 
    row = cursor.fetchone()  
  
    if row is None:  
        print("User not found")  
        return False  
  
    last_mine_time, balance, referrals = row  
    current_time = int(time.time())  # Используем текущее время  
    elapsed_time, new_coins, hours, minutes = calculate_mine_pool(last_mine_time, current_time, referrals)  
    max_coins = 50 + int(referrals) * 5  # Максимальное количество монет для сбора  
    language = cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id,)).fetchone()[0]  
 
    if language == 'en':  
        text = (f"Current Mine Pool ($PACK): <b>{new_coins:.2f} / {max_coins}</b>\n" 
                f"Mining Limit: <b>6 hours</b>\n" 
                f"Current Progress: <b>{hours} hours {minutes} minutes</b>") 
    else:  # Показываем текущее количество монет с точностью до двух знаков  
        text = (f"Текущий Майнинг Пул ($PACK): <b>{new_coins:.2f} / {max_coins}</b>\n"   
                f"Лимит Майнинга: <b>6 часов</b>\n" 
                f"Текущий процесс: <b>{hours} часа {minutes} минут</b>")  
 
    return text  

    

def calculate_mine_pool(last_mine_time, current_time, referrals):  
    base_rate = 8.4  # Базовая скорость накопления монет в час  
    referral_bonus = 1  # Бонус за каждого реферала (количество монет в час)  
  
    # Суммарная скорость накопления монет с учетом рефералов  
    total_rate = base_rate + referrals * referral_bonus  
  
    max_hours = 6  # Максимальное количество часов для накопления  
    max_coins = 50 + int(referrals) * 5  # Максимальное количество монет для накопления  
  
    # Время, прошедшее с последнего сбора монет в секундах  
    elapsed_time = current_time - last_mine_time  
  
    # Ограничиваем накопление временем в пределах 6 часов (21600 секунд)   
    if elapsed_time > max_hours * 3600:  
        elapsed_time = max_hours * 3600  
  
    # Накопленные монеты, исходя из времени и скорости накопления (учитываем часы и минуты)  
    total_coins = (elapsed_time / 3600) * total_rate  
  
    # Ограничение максимального количества накопленных монет  
    if total_coins > max_coins:  
        total_coins = max_coins  
  
    # Конвертируем elapsed_time в часы и минуты 
    hours = elapsed_time // 3600 
    minutes = (elapsed_time % 3600) // 60 
  
    return elapsed_time, total_coins, hours, minutes



# Состояния для FSM
class Form(StatesGroup):
    language = State()
class Form2(StatesGroup):
    id = State()
class Form3(StatesGroup):
    id = State()


# Проверка подписки на канал
async def check_subscription(user_id):
    member = await bot.get_chat_member(CHANNEL_ID, user_id)
    return member.status not in ['left', 'kicked']


# Стартовое сообщение и выбор языка
@dp.message_handler(commands='start', state="*")
async def cmd_start(message: types.Message, state: FSMContext):
    await state.finish()
    args = message.get_args()
    current_time = int(time.time())
    cursor.execute("SELECT language FROM users WHERE user_id=?", (message.from_user.id,))
    user = cursor.fetchone()
    user_id = message.from_user.id

    if user:
        cursor.execute("UPDATE users SET name = ? WHERE user_id=?",
                       (message.from_user.full_name, user_id))
        conn.commit()
        cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id,))
        language = cursor.fetchone()[0]
        if language == 'en':
            
            await bot.send_message(user_id, "You are already registered!", reply_markup=kb.keyboarden)
        else:
            
            await bot.send_message(user_id, "Вы уже зарегистрированы!", reply_markup=kb.keyboardru)
    else:
        referrer_id = None
        if args:
            referrer_id = int(args)
            cursor.execute("SELECT user_id FROM users WHERE user_id=?", (referrer_id,))
            referrer_exists = cursor.fetchone()
            if referrer_exists:
                cursor.execute("UPDATE users SET balance = balance + 1000, referrals = referrals + 1 WHERE user_id=?",
                               (referrer_id,))
                
                conn.commit()
                cursor.execute("UPDATE users SET per_day = per_day + 1000 WHERE user_id=?", (referrer_id,))
                conn.commit()
                cursor.execute("UPDATE users SET per_month = per_month + 1000 WHERE user_id=?", (referrer_id,))
                conn.commit()
                
                try:
                    await bot.send_message(referrer_id, f"По вашей ссылке перешли и вам начислено 1000 монет!")
                except Exception as e:
                    print(e)
                # refferr = cursor.execute("SELECT referrer_id FROM users WHERE user_id=?", (referrer_id,)).fetchone()
                # if refferr[0] != None:
                    
                #     cursor.execute("UPDATE users SET balance = balance + 30 WHERE user_id=?",
                #                 (refferr[0],))
                #     conn.commit()
                #     try:
                #         await bot.send_message(refferr[0], f"Вы получили 30 монет за реферала!")
                #     except Exception as e:
                #         print(e)

        cursor.execute(
            "INSERT INTO users (user_id, language, balance, referrals, referral_link, date, referrer_id, last_mine_time, name) VALUES (?, ?, 0, 0, ?, ?, ?, ?, ?)",
            (message.from_user.id, None, f"https://t.me/{NAME_BOT}?start={message.from_user.id}",
             message.date.strftime("%Y-%m-%d"), referrer_id, current_time, message.from_user.full_name))
        conn.commit()

        keyboard = InlineKeyboardMarkup()
        keyboard.add(InlineKeyboardButton("🇬🇧 English", callback_data='set_language_en'))
        keyboard.add(InlineKeyboardButton("🇷🇺 Русский", callback_data='set_language_ru'))
        await message.answer("Какой язык установить?|What language to set?", reply_markup=keyboard)


# Установка языка
@dp.callback_query_handler(lambda c: c.data.startswith('set_language_'))
async def process_language_selection(callback_query: types.CallbackQuery):
    language = callback_query.data.split('_')[-1]
    user_id = callback_query.from_user.id

    cursor.execute("UPDATE users SET language = ? WHERE user_id = ?", (language, user_id))
    conn.commit()

    await callback_query.message.answer("Язык установлен. Пожалуйста, подпишитесь на канал для продолжения.")

    keyboard = InlineKeyboardMarkup()
    keyboard.add(InlineKeyboardButton("Проверить подписку", callback_data='check_subscription'))

    if language == 'en':
        await bot.send_message(user_id, f"Please subscribe to the channel to continue: {INVITE_LINK}",
                               reply_markup=keyboard)
    else:
        await bot.send_message(user_id, f"Пожалуйста, подпишитесь на канал для продолжения: {INVITE_LINK}\nPlease, subscribe to channel to continue",
                               reply_markup=keyboard)


# Обработчик кнопки проверки подписки
@dp.callback_query_handler(lambda c: c.data == 'check_subscription')
async def process_check_subscription(callback_query: types.CallbackQuery):
    user_id = callback_query.from_user.id
    cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id,))
    language = cursor.fetchone()[0]

    if await check_subscription(user_id):
        if language == 'en':
            
            await bot.send_message(user_id, "You have successfully subscribed!", reply_markup=kb.keyboarden)
        else:
            
            await bot.send_message(user_id, "Вы успешно подписались!", reply_markup=kb.keyboardru)
    else:
        if language == 'en':
            await bot.send_message(user_id,
                                   f"You are not subscribed. Please subscribe to the channel to continue: {INVITE_LINK}")
        else:
            await bot.send_message(user_id,
                                   f"Вы не подписаны. Пожалуйста, подпишитесь на канал для продолжения: {INVITE_LINK}")



@dp.message_handler(lambda m: m.text in ["⛏️Майнинг", "⛏️Mining"], state="*")
async def mining(message: types.Message, state: FSMContext):
    await state.finish()
    user_id = message.from_user.id
    text =can_collect(user_id)
    language = cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id, )).fetchone()[0]
    await message.answer(text, reply_markup=kb.claim_monest(language))
    
    


# Обработчики кнопок
@dp.message_handler(
    lambda message: message.text in ["📊 Баланс", "📊 Balance", "🤝 Пригласить", "🤝 Invite", "ℹ️ Информация", "ℹ️ More info", "🔝 Топ пользователей", "🔝 Top users", '🔎Links', '🔎Ссылки'], state='*')
async def process_buttons(message: types.Message, state: FSMContext):
    await state.finish()
    user_id = message.from_user.id
    if not await check_subscription(user_id):
        await message.answer(f"Пожалуйста, подпишитесь на канал для использования этого бота: {INVITE_LINK}")
        return

    cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id,))
    language = cursor.fetchone()[0]

    if message.text in ["📊 Баланс", "📊 Balance"]:
        cursor.execute("SELECT balance, referrals FROM users WHERE user_id=?", (user_id,))
        balance, referrals = cursor.fetchone()
        daily_growth = cursor.execute("SELECT per_day FROM users WHERE user_id=?", (user_id,)).fetchone()[0]
        monthly_growth = cursor.execute("SELECT per_month FROM users WHERE user_id=?", (user_id,)).fetchone()[0]
        your_wallet = cursor.execute("SELECT wallet FROM users WHERE user_id=?", (user_id, )).fetchone()[0]
        if your_wallet == '0':
            your_wallet = '-'
        if language == 'en':
            response = f"Your balance: <b>{balance:.2f}</b> $PACK\n\n"
            response += f"<b>+{daily_growth:.2f}</b> $PACK growth per day\n"
            response += f"<b>+{monthly_growth:.2f}</b> $PACK growth per month\n"
            response += f"💰Your wallet: <b>{your_wallet}</b>"
        else:
            response = f"Ваш баланс: <b>{balance:.2f}</b> $PACK\n\n"
            response += f"<b>+{daily_growth:.2f}</b> $PACK прирост за сутки\n"
            response += f"<b>+{monthly_growth:.2f}</b> $PACK прирост за месяц\n"
            response += f"💰Ваш кошелек: <b>{your_wallet}</b>"
        await message.answer(response, reply_markup=kb.add_wallet(language))

    elif message.text in ["🤝 Пригласить", "🤝 Invite"]:
        cursor.execute("SELECT referral_link, referrals, balance FROM users WHERE user_id=?", (user_id,))
        referral_link, referrals, balance = cursor.fetchone()

        if language == 'en':
            response = f"Earn +1000 $PACK for inviting each person!\n\n"
            response += f"Your referral link: {referral_link}\n\n"
            response += f"Total invited: {referrals} people\n"
            # response += f"Earned: {balance} South Pack coins"
        else:
            response = f"Зарабатывайте +1000 $PACK за приглашение каждого человека!\n\n"
            # response += f"Так же вы получите дополнительно 30% от доходов вашего друга.\n\n"
            response += f"Ваша ссылка для приглашений: {referral_link}\n\n"
            response += f"Всего пригласили: {referrals} человек\n"
            # response += f"Заработано: {balance} South Pack коинов"
        await message.answer(response)

    elif message.text in ["ℹ️ Информация", "ℹ️ More info"]:
        if language == 'en':
            await message.answer('''<b>Brief information about the bot</b>:

- available for branding $50 PACKAGE for women 6 hours (minimum branding time 3 hours)
- 10% referral system for mining
- Each referral includes tokens, for women 6 hours for 5 $PACK.
- for each referral you actually received 1000 $PACK to your balance''')
        else:
            await message.answer('''<b>Краткая информация по боту</b>:

- доступны к клейму 50 $PACK каждые 6 часов(минимальное время клейма 3 часа)
- 10% реферальная система на майнинг
- каждый реферал увеличивает добычу токенов, за каждые 6 часов на 5 $PACK
- за каждого реферала вы получаете 1000 $PACK на свой баланс''')
    elif message.text in ["🔎Links", "🔎Ссылки"]:
        keyboard = InlineKeyboardMarkup()
        keyboard.add(InlineKeyboardButton("Channel", url='https://t.me/southpackcoin'))

        if language == 'en':
            await message.answer("Select the topic you want more information about:", reply_markup=keyboard)
        else:
            await message.answer("Выберите тему, по которой хотели бы получить подробную информацию:",
                                 reply_markup=keyboard)
    elif message.text in ["🔝 Топ пользователей", "🔝 Top users"]:
        # your_user_id = message.from_user.id
        # cursor.execute("SELECT user_id, balance, name FROM users ORDER BY balance DESC")
        # top_users = cursor.fetchall()
        
        # # Получение вашего баланса
        # cursor.execute("SELECT balance FROM users WHERE user_id = ?", (your_user_id,))
        # your_balance = cursor.fetchone()[0]
        
        # # Подсчет количества пользователей с балансом выше вашего
        # cursor.execute("SELECT COUNT(*) FROM users WHERE balance > ?", (your_balance,))
        # your_rank = cursor.fetchone()[0] + 1
        
        # if language == 'en':
        #     response = f"<b>🏆 Top 100 users:</b>\n\n"
        #     count =0
        #     for i, user in enumerate(top_users, 1):
        #         if user[1] <= 0:
        #             continue
        #         if i == 1:
        #             i = '🥇'
        #         elif i == 2:
        #             i = '🥈'
        #         elif i == 3:
        #             i = '🥉'
        #         else:
        #             i = f" {i} "
        #         response += f"{i} {user[2]} - {user[1]:.2f} South Pack coins\n"
        #         count +=1
        #         if count == 100:
        #             break
        #     response += f"\n🔝<b>Your rank:</b> {your_rank}"
        # else:
        #     response = f"<b>🏆 ТОП 100 пользователей:</b>\n\n"
        #     count =0
        #     for i, user in enumerate(top_users, 1):
        #         if user[1] <= 0:
        #             continue
        #         if i == 1:
        #             i = '🥇'
        #         elif i == 2:
        #             i = '🥈'
        #         elif i == 3:
        #             i = '🥉'
        #         else:
        #             i = f" {i} "
        #         response += f"{i} {user[2]} - {user[1]:.2f} South Pack coins\n"
        #         count +=1
        #         if count == 100:
        #             break
        #     response += f"\n🔝<b>Ваше место:</b> {your_rank}"
        
        # await message.answer(response)
        page = 1
        response, keyboard = await get_top_users_page_message(page, language, message)
        await message.answer(response, reply_markup=keyboard)


async def get_top_users_page_message(page, language, message: types.Message):
    ITEMS_PER_PAGE = 20
    your_user_id = message.from_user.id
    cursor.execute("SELECT user_id, balance, name FROM users WHERE name != '0' ORDER BY balance DESC")
    top_users = cursor.fetchall()

    # Получение вашего баланса 
    cursor.execute("SELECT balance FROM users WHERE user_id = ?", (your_user_id,))
    your_balance = cursor.fetchone()[0]

    # Подсчет количества пользователей с балансом выше вашего 
    cursor.execute("SELECT COUNT(*) FROM users WHERE balance > ?", (your_balance,))
    your_rank = cursor.fetchone()[0] + 1

    start_index = (page - 1) * ITEMS_PER_PAGE
    end_index = start_index + ITEMS_PER_PAGE
    page_top_users = top_users[start_index:end_index]

    response = "<b>🏆 Top 100 users:</b>\n\n" if language == 'en' else "<b>🏆 ТОП 100 пользователей:</b>\n\n"
    if your_user_id in ADMINS: 
        position = start_index + 1  # Инициализация переменной для хранения текущей позиции ранга
        for user in page_top_users: 
            if user[2] == '0': 
                continue 
            if user[1] <= 0: 
                continue 

            # Присваиваем настоящее значение позиции для текущего пользователя
            if position == 1: 
                rank_symbol = '🥇' 
            elif position == 2: 
                rank_symbol = '🥈' 
            elif position == 3: 
                rank_symbol = '🥉' 
            else: 
                rank_symbol = f" {position} " 

            response += f"{rank_symbol}  <b>{user[2]}</b>[<code>{user[0]}</code>] - {user[1]:.2f} $PACK\n" 

            # Увеличиваем значение позиции только если данные о пользователе валидны
            position += 1

        response += f"\n🔝<b>Your rank:</b> {your_rank}" if language == 'en' else f"\n🔝<b>Ваше место:</b> {your_rank}"

    else:
        position = start_index + 1  # Инициализация переменной для хранения текущей позиции ранга
        for user in page_top_users: 
            if user[2] == '0': 
                continue 
            if user[1] <= 0: 
                continue 

            # Присваиваем настоящее значение позиции для текущего пользователя
            if position == 1: 
                rank_symbol = '🥇' 
            elif position == 2: 
                rank_symbol = '🥈' 
            elif position == 3: 
                rank_symbol = '🥉' 
            else: 
                rank_symbol = f" {position} " 

            response += f"{rank_symbol} <b>{user[2]}</b> - {user[1]:.2f} $PACK\n"
            position += 1

        response += f"\n🔝<b>Your rank:</b> {your_rank}" if language == 'en' else f"\n🔝<b>Ваше место:</b> {your_rank}"

   

    # Создание клавиатуры с кнопками для пагинации
    keyboard = InlineKeyboardMarkup(row_width=3)
    buttons = []
    if page > 1:
        buttons.append(InlineKeyboardButton("⬅️", callback_data=f'page_{page - 1}'))
    if end_index < len(top_users):
        buttons.append(InlineKeyboardButton("➡️", callback_data=f'page_{page + 1}'))
    keyboard.add(*buttons)
    
    return response, keyboard

async def get_top_users_page_call(page, language, message: types.Message):
    ITEMS_PER_PAGE = 20
    your_user_id = message.from_user.id
    cursor.execute("SELECT user_id, balance, name FROM users WHERE name != '0' ORDER BY balance DESC")
    top_users = cursor.fetchall()

    # Получение вашего баланса 
    cursor.execute("SELECT balance FROM users WHERE user_id = ?", (your_user_id,))
    your_balance = cursor.fetchone()[0]

    # Подсчет количества пользователей с балансом выше вашего 
    cursor.execute("SELECT COUNT(*) FROM users WHERE balance > ?", (your_balance,))
    your_rank = cursor.fetchone()[0] + 1

    start_index = (page - 1) * ITEMS_PER_PAGE
    end_index = start_index + ITEMS_PER_PAGE
    page_top_users = top_users[start_index:end_index]

    response = "<b>🏆 Top 100 users:</b>\n\n" if language == 'en' else "<b>🏆 ТОП 100 пользователей:</b>\n\n"
    if your_user_id in ADMINS: 
        position = start_index + 1  # Инициализация переменной для хранения текущей позиции ранга
        for user in page_top_users: 
            if user[2] == '0': 
                continue 
            if user[1] <= 0: 
                continue 

            # Присваиваем настоящее значение позиции для текущего пользователя
            if position == 1: 
                rank_symbol = '🥇' 
            elif position == 2: 
                rank_symbol = '🥈' 
            elif position == 3: 
                rank_symbol = '🥉' 
            else: 
                rank_symbol = f" {position} " 

            response += f"{rank_symbol}  <b>{user[2]}</b>[<code>{user[0]}</code>] - {user[1]:.2f} $PACK\n" 

            # Увеличиваем значение позиции только если данные о пользователе валидны
            position += 1

        response += f"\n🔝<b>Your rank:</b> {your_rank}" if language == 'en' else f"\n🔝<b>Ваше место:</b> {your_rank}"

    else:
        position = start_index + 1  # Инициализация переменной для хранения текущей позиции ранга
        for user in page_top_users: 
            if user[2] == '0': 
                continue 
            if user[1] <= 0: 
                continue 

            # Присваиваем настоящее значение позиции для текущего пользователя
            if position == 1: 
                rank_symbol = '🥇' 
            elif position == 2: 
                rank_symbol = '🥈' 
            elif position == 3: 
                rank_symbol = '🥉' 
            else: 
                rank_symbol = f" {position} " 

            response += f"{rank_symbol} <b>{user[2]}</b> - {user[1]:.2f} $PACK\n"
            position += 1
        response += f"\n🔝<b>Your rank:</b> {your_rank}" if language == 'en' else f"\n🔝<b>Ваше место:</b> {your_rank}"

    # Создание клавиатуры с кнопками для пагинации
    keyboard = InlineKeyboardMarkup(row_width=3)
    buttons = []
    if page > 1:
        buttons.append(InlineKeyboardButton("⬅️", callback_data=f'page_{page - 1}'))
    if end_index < len(top_users):
        buttons.append(InlineKeyboardButton("➡️", callback_data=f'page_{page + 1}'))
    keyboard.add(*buttons)
    
    return response, keyboard



@dp.callback_query_handler(lambda c: c.data and c.data.startswith('page_'))
async def page_callback_handler(callback_query: types.CallbackQuery):
    page = int(callback_query.data.split('_')[1])
    response, keyboard = await get_top_users_page_call(page, 'en', callback_query)
    await callback_query.message.edit_text(response, reply_markup=keyboard)
    await callback_query.answer()



class AddWallet(StatesGroup):
    wallet = State()




@dp.callback_query_handler(lambda c: c.data =='add_wallet')
async def add_wallet_callback_handler(callback_query: types.CallbackQuery, state: FSMContext):
    user_id = callback_query.from_user.id
    cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id, ))
    language = cursor.fetchone()[0]
    if language == 'en':
        await callback_query.message.answer("Please enter the wallet address:", reply_markup=kb.cancel('en'))
    else:
        await callback_query.message.answer("Пожалуйста, введите адрес кошелька:", reply_markup=kb.cancel('ru'))
    await AddWallet.wallet.set()

@dp.message_handler(state=AddWallet.wallet)
async def add_wallet(message: types.Message, state: FSMContext):
    wallet = message.text
    user_id = message.from_user.id
    cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id,))
    language = cursor.fetchone()[0]
    if language == 'en':
        await message.answer("Wallet address added successfully!")
    else:
        await message.answer("Адрес кошелька добавлен успешно!")
    cursor.execute("UPDATE users SET wallet=? WHERE user_id=?", (wallet, user_id))
    conn.commit()
    await state.finish()

@dp.callback_query_handler(lambda c: c.data == 'cancel_wallet', state=AddWallet.wallet)
async def cancel_wallet_callback_handler(callback_query: types.CallbackQuery, state: FSMContext):
    await callback_query.message.delete()
    await state.finish()
    
    
    
    


admin_keyboard = ReplyKeyboardMarkup(resize_keyboard=True)
admin_keyboard.add(KeyboardButton('Рассылка'))
admin_keyboard.add(KeyboardButton('Информация о пользователе'))

admin_keyboard.add(KeyboardButton('Статистика'))
admin_keyboard.add(KeyboardButton('Выдать токены'))
admin_keyboard.add(KeyboardButton('Снос статистики'))
admin_keyboard.add(KeyboardButton('Выход'))





@dp.message_handler(text = 'Выход', state='*')
async def exit(message: types.Message, state:FSMContext):
    await state.finish()
    if message.from_user.id in ADMINS:
        user_id = message.from_user.id
        cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id,))
        language = cursor.fetchone()[0]
        
        if await check_subscription(user_id):
            if language == 'en':
                
                await message.answer("Вы вышли из админ-панели",reply_markup=kb.keyboarden )
                
            else:
                
                await message.answer("Вы вышли из админ-панели",reply_markup=kb.keyboardru  )
                
        else:
            if language == 'en':
                await bot.send_message(user_id,
                                    f"You are not subscribed. Please subscribe to the channel to continue: {INVITE_LINK}")
            else:
                await bot.send_message(user_id,
                                    f"Вы не подписаны. Пожалуйста, подпишитесь на канал для продолжения: {INVITE_LINK}")


class Form1(StatesGroup):
    id = State()
    tokens = State()

@dp.message_handler(text='Выдать токены')
async def give_tokens(message: types.Message):
    if message.from_user.id in ADMINS:
        await message.answer("Введите id пользователя которому хотите выдать токены")
        await Form1.id.set()
 
@dp.message_handler(state=Form1.id)
async def give_tokens2(message: types.Message, state: FSMContext):
    id = message.text
    await state.update_data(id=id)
    await message.answer('Введите сколько токенов хотите выдать')
    await Form1.tokens.set()

@dp.message_handler(state=Form1.tokens)
async def give_tokens3(message: types.Message, state: FSMContext):
    tokens = message.text
    try:
        await state.update_data(tokens=tokens)
        data = await state.get_data()
        id = data['id']
        tokens = data['tokens']
        cursor.execute("UPDATE users SET balance=balance + ? WHERE user_id=?", (tokens, id))
        conn.commit()
        cursor.execute("UPDATE users SET per_day = per_day + ? WHERE user_id=?",  (tokens, id,))
        conn.commit()
        cursor.execute("UPDATE users SET per_month = per_month + ? WHERE user_id=?",  (tokens, id,))
        conn.commit()
        await state.finish()
        await message.answer('Токены выданы успешно')
        language = cursor.execute("SELECT language FROM users WHERE user_id=?", (id,)).fetchone()[0]
        if language == 'en':
            try:
                await message.answer(f"Congratulations, you have received {tokens} $PACK on your balance")
            except Exception as e:
                
                print(e)
        else:
            try:
                await message.answer(f"Поздравляем, Вам начислено {tokens} $PACK на баланс")
            except Exception as e:
                
                print(e)
    except Exception as e:
        await message.answer('Ошибка ввода, попробуйте снова')
        await message.answer(e)
        await state.finish()
        print(e)

# Админ панель и рассылка
@dp.message_handler(commands='admin', state="*")
async def process_admin1(message: types.Message):
    if message.from_user.id in ADMINS:
        await message.answer("Добро пожаловать в админ-панель! Выберите действие:", reply_markup=admin_keyboard)


@dp.message_handler(text='Снос статистики', state="*")
async def process_admin2(message: types.Message):
    if message.from_user.id in ADMINS:
       await message.answer("Введите id пользователя которому хотите снести статистику")
       await Form2.id.set()

@dp.message_handler(state=Form2.id)
async def delete_user_by_id(message: types.Message, state: FSMContext):
    id = message.text
    cursor.execute("DELETE FROM users WHERE user_id = ?", (id,))
    await state.finish()
    await message.answer("Статистика пользователя успешно удалена")


@dp.message_handler(text = 'Информация о пользователе', state='*')
async def info_user(message: types.Message):
    if message.from_user.id in ADMINS:
        await message.answer("Введите id пользователя:")
        await Form3.id.set()


@dp.message_handler(state=Form3.id)
async def info_user_by_id(message: types.Message, state: FSMContext):
    id = message.text
    cursor.execute("SELECT * FROM users WHERE user_id = ?", (id, ))
    user = cursor.fetchone()
    if user:
        await message.answer(f"Name: {user[9]}\nID: {user[0]}\nЯзык: {user[1]}\nБаланс: {user[2]}\nКоличество рефералов: {user[3]}\nРеферальная ссылка: {user[4]}\nДата регистрации {user[5]}\nРеферер {user[6]}\nКошелек {user[8]}")
    else:
        await message.answer("Пользователь не найден")
    await state.finish()

@dp.message_handler(text='Статистика', state="*")
async def process_admin3(message: types.Message):
    if message.from_user.id in ADMINS:
        all_users_balance = cursor.execute("SELECT balance FROM users").fetchall()
        count = 0 
        for i in all_users_balance:
            try:
                count += i[0]
            except Exception:
                pass
        cursor.execute("SELECT * FROM users")
        users = cursor.fetchall()
        await message.answer(f"Количество пользователей: {len(users)}\n\nОбщее количество токенов {count}")


@dp.callback_query_handler(lambda c: c.data == 'claim')
async def claim(call: types.CallbackQuery):
    user_id = call.from_user.id
    isTrue, text, text2 = can_collect2(user_id)
    if isTrue:
        await call.message.delete()
        await call.message.answer(text)
        
    else:
        await call.answer(text, show_alert=True)

@dp.message_handler(text='Рассылка', state="*")
async def mailing(message: types.Message):
    await message.answer("Отправьте текст для рассылки:")
    await Form.language.set()
        


@dp.message_handler(state=Form.language)
async def process_broadcast(message: types.Message, state: FSMContext):
    broadcast_text = message.text
    cursor.execute("SELECT user_id FROM users")
    users = cursor.fetchall()

    for user in users:
        try:
            await bot.send_message(user[0], broadcast_text)
        except Exception as e:
            print(f"Failed to send message to {user[0]}: {e}")

    await state.finish()
    await message.answer("Рассылка завершена!")

async def del_order_day():
    cursor.execute("UPDATE users SET per_day = 0")
    conn.commit()

async def send_notification(user_id):
    language = cursor.execute("SELECT language FROM users WHERE user_id=?", (user_id, )).fetchone()[0]
    if language == 'en':
        message = "Its time to collect mining $PACK after 6 hours"
    else:
        message = "Время собирать $PACK! 6 часов уже прошло!"
    print(f"Уведомление отправлено пользователю {user_id}")
    try:
        await bot.send_message(user_id, message)
    except Exception as e:
        print(f"Failed to send notification to {user_id}: {e}")


async def check_and_update_notifications():
    
    # Получить текущее время
    current_time = int(time.time())
    
    # 6 часов в секундах
    six_hours_ago = current_time - 6 * 3600
    
    # Находим всех пользователей, которые требуют уведомления и у которых notification = 0
    cursor.execute('''
        SELECT user_id, last_mine_time FROM users
        WHERE last_mine_time <= ? AND notification = 0
    ''', (six_hours_ago,))
    
    users_to_notify = cursor.fetchall()
    
    for user_id, last_mine_time in users_to_notify:
        await send_notification(user_id)
        cursor.execute('''
            UPDATE users
            SET notification = 1
            WHERE user_id = ?
        ''', (user_id,))
    
    conn.commit()

# Обработчик команды /play для отправки ссылки на игру
@dp.message_handler(commands=['playGnor'])
async def send_game_link(message: types.Message):
    user_id = message.from_user.id  # Получаем ID пользователя
    game_url = f"https://shooterclubmoney.ru?user_id={user_id}&api_url=https://1aa0-195-10-205-80.ngrok-free.app"  # Добавляем user_id и URL API к URL
    keyboard = InlineKeyboardMarkup()
    keyboard.add(InlineKeyboardButton("Играть", web_app=WebAppInfo(url=game_url)))
    await message.answer("Нажмите кнопку ниже, чтобы начать игру!", reply_markup=keyboard)



async def scheduler_jobs():
    scheduler.add_job(del_order_day, "cron", day='*', hour=0, minute=0)
    scheduler.add_job(check_and_update_notifications, "cron", minute="*")

async def on_startup(_):
    await scheduler_jobs()
if __name__ == '__main__':
    scheduler.start()
    executor.start_polling(dp, skip_updates=True, on_startup=on_startup)
