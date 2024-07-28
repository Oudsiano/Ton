from datetime import datetime
from flask import Flask, request, jsonify
from flask_cors import CORS
import sqlite3

app = Flask(__name__)
CORS(app, resources={r"/api/*": {"origins": "*"}})  # Разрешить CORS для всех маршрутов API
db_path = '/root/Botsqyzz/users.db'  # Указан путь к вашей базе данных

@app.route('/api/upgrade/<id>', methods=['GET'])
def get_user_upgrade(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT 
                energy, 
                energytime, 
                referral_link, 
                balance, 
                level_boss, 
                count_blows,
                weapon,
                soft_coins
            FROM users 
            WHERE user_id = ?
        ''', (id,))
        row = cursor.fetchone()

        if row:
            server_time = datetime.utcnow().timestamp()
            energytime = float(row[1])
            current_energy = float(row[0])
            
            # Приведение к int, если weapon и soft_coins хранятся как float
            weapon = int(row[6])
            havemoney = int(row[7])

            upgrade_costs = [30, 100, 300, 500, 1000, 5000]
            cost = upgrade_costs[weapon]

            if server_time > energytime:
                # Вычисляем разницу времени в секундах
                time_diff = server_time - energytime
                remainder = time_diff % (6 * 3600)

                # Обновляем energytime и energy
                new_energytime = server_time + (6 * 3600 - remainder)
                new_energy = 20
                
                cursor.execute('''
                    UPDATE users
                    SET energy = ?, energytime = ?
                    WHERE user_id = ?
                ''', (new_energy, new_energytime, id))
                
                conn.commit()
            else:
                new_energy = current_energy
                new_energytime = energytime

            if havemoney >= cost:
                # Обновляем weapon и вычитаем стоимость из soft_coins
                new_weapon = weapon + 1
                new_soft_coins = havemoney - cost
                
                cursor.execute('''
                    UPDATE users
                    SET weapon = ?, soft_coins = ?
                    WHERE user_id = ?
                ''', (new_weapon, new_soft_coins, id))
                
                conn.commit()
            else:
                new_weapon = weapon
                new_soft_coins = havemoney

            conn.close()
            return jsonify({
                'energy': new_energy,
                'energytime': new_energytime,
                'referral_link': row[2],
                'balance': row[3],
                'level_boss': row[4],
                'count_blows': row[5],
                'weapon': new_weapon,
                'soft_coins': new_soft_coins,
                'server_time': server_time
            })
        else:
            conn.close()
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500



@app.route('/api/click/<id>', methods=['GET'])
def get_user_click(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT 
                energy, 
                energytime, 
                referral_link, 
                balance, 
                level_boss, 
                count_blows,
                weapon,
                soft_coins
            FROM users 
            WHERE user_id = ?
        ''', (id,))
        row = cursor.fetchone()

        if row:
            server_time = datetime.utcnow().timestamp()
            energytime = float(row[1])
            current_energy = row[0]
            weapon = row[6]

            increment = weapon + 1

            # Update soft_coins, balance, and count_blows by increment
            new_soft_coins = row[7] + increment
            new_balance = row[3] + increment
            new_count_blows = row[5] + increment

            cursor.execute('''
                UPDATE users
                SET soft_coins = ?, balance = ?, count_blows = ?
                WHERE user_id = ?
            ''', (new_soft_coins, new_balance, new_count_blows, id))

            if server_time > energytime:
                time_diff = server_time - energytime
                remainder = time_diff % (6 * 3600)

                new_energytime = server_time + (6 * 3600 - remainder)
                new_energy = 20
                
                cursor.execute('''
                    UPDATE users
                    SET energy = ?, energytime = ?
                    WHERE user_id = ?
                ''', (new_energy, new_energytime, id))
            else:
                new_energy = current_energy

            if new_energy > 0:
                new_energy -= 1
                cursor.execute('''
                    UPDATE users
                    SET energy = ?
                    WHERE user_id = ?
                ''', (new_energy, id))
            
            conn.commit()
            conn.close()

            return jsonify({
                'energy': new_energy,
                'energytime': new_energytime if server_time > energytime else energytime,
                'referral_link': row[2],
                'balance': new_balance,
                'level_boss': row[4],
                'count_blows': new_count_blows,
                'weapon': weapon,
                'soft_coins': new_soft_coins,
                'server_time': server_time
            })
        else:
            conn.close()
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500




@app.route('/api/data/<id>', methods=['GET'])
def get_user_data(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        
        cursor.execute('''
            SELECT 
                energy, 
                energytime, 
                referral_link, 
                balance, 
                level_boss, 
                count_blows,
                weapon,
                soft_coins
            FROM users 
            WHERE user_id = ?
        ''', (id,))
        row = cursor.fetchone()

        if row:
            server_time = datetime.utcnow().timestamp()
            energytime = float(row[1])

            if server_time > energytime:
                # Вычисляем разницу времени в секундах
                time_diff = server_time - energytime
                remainder = time_diff % (6 * 3600)

                # Обновляем energytime и energy
                new_energytime = server_time + (6 * 3600 - remainder)
                new_energy = 20
                
                cursor.execute('''
                    UPDATE users
                    SET energy = ?, energytime = ?
                    WHERE user_id = ?
                ''', (new_energy, new_energytime, id))
                
                conn.commit()
                conn.close()

                return jsonify({
                    'energy': new_energy,
                    'energytime': new_energytime,
                    'referral_link': row[2],
                    'balance': row[3],
                    'level_boss': row[4],
                    'count_blows': row[5],
					'weapon': row[6],
                    'soft_coins': row[7],
                    'server_time': server_time
                })
            else:
                conn.close()
                return jsonify({
                    'energy': row[0],
                    'energytime': energytime,
                    'referral_link': row[2],
                    'balance': row[3],
                    'level_boss': row[4],
                    'count_blows': row[5],
					'weapon': row[6],
                    'soft_coins': row[7],
                    'server_time': server_time
                })
        else:
            conn.close()
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500



@app.route('/api/energy/<id>', methods=['GET'])
def get_energy(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('SELECT energy FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'energy': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/energytime/<id>', methods=['GET'])
def get_energytime(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('SELECT energytime FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'energytime': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500


@app.route('/api/referral/<id>', methods=['GET'])
def get_referral_link(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('SELECT referral_link FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'referral_link': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/coins/<id>', methods=['GET'])
def get_coins(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('SELECT balance FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'balance': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/coins/<id>', methods=['POST'])
def update_coins(id):
    try:
        data = request.get_json()
        coins = data.get('coins')
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('UPDATE users SET balance = balance + ? WHERE user_id = ?', (coins, id))
        conn.commit()
        cursor.execute('SELECT balance FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'balance': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/level_boss/<id>', methods=['GET'])
def get_level_boss(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('SELECT level_boss FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'level_boss': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/level_boss/<id>', methods=['POST'])
def update_level_boss(id):
    try:
        data = request.get_json()
        level_boss = data.get('level_boss')
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('UPDATE users SET level_boss = ? WHERE user_id = ?', (level_boss, id))
        conn.commit()
        cursor.execute('SELECT level_boss FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'level_boss': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/count_blows/<id>', methods=['GET'])
def get_count_blows(id):
    try:
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('SELECT count_blows FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'count_blows': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/count_blows/<id>', methods=['POST'])
def update_count_blows(id):
    try:
        data = request.get_json()
        count_blows = data.get('count_blows')
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()
        cursor.execute('UPDATE users SET count_blows = ? WHERE user_id = ?', (count_blows, id))
        conn.commit()
        cursor.execute('SELECT count_blows FROM users WHERE user_id = ?', (id,))
        row = cursor.fetchone()
        conn.close()
        if row:
            return jsonify({'count_blows': row[0]})
        else:
            return jsonify({'error': 'User not found'}), 404
    except Exception as e:
        return jsonify({'error': str(e)}), 500

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=True)

